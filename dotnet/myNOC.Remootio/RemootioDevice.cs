using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace myNOC.Remootio;

/// <summary>
/// Low-level Remootio WebSocket client. Ports remootioDevice.ts from the remootio-api-client-node library.
/// Call RunAsync() to connect, authenticate, and process messages until disconnected.
/// SendOpen() / SendClose() may be called at any time from any thread.
/// </summary>
public sealed class RemootioDevice
{
    private readonly RemootioDeviceConfig _config;
    private readonly ILogger _logger;

    private string? _apiSessionKey;
    private int? _lastActionId;
    private bool _waitingForAuthQueryResponse;
    private Channel<string>? _sendChannel;

    public event Action<bool, bool>? ConnectionChanged;  // (connected, authenticated)
    public event Action<GateState>? GateStateReceived;

    public RemootioDevice(RemootioDeviceConfig config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Connects to the device, authenticates, and processes messages.
    /// Returns when the connection is closed or the cancellation token is cancelled.
    /// </summary>
    public async Task RunAsync(CancellationToken ct)
    {
        _apiSessionKey = null;
        _lastActionId = null;
        _waitingForAuthQueryResponse = false;

        _sendChannel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true });

        using var ws = new ClientWebSocket();
        var uri = new Uri($"ws://{_config.DeviceIp}:8080/");

        try
        {
            await ws.ConnectAsync(uri, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning("Could not connect to Remootio at {Uri}: {Message}", uri, ex.Message);
            _sendChannel = null;
            return;
        }

        _logger.LogInformation("Connected to Remootio at {Uri}", uri);
        ConnectionChanged?.Invoke(true, false);

        // Kick off authentication immediately
        Enqueue("{\"type\":\"AUTH\"}");

        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);

        var receiveTask = ReceiveLoopAsync(ws, linked.Token);
        var sendTask = SendLoopAsync(ws, linked.Token);
        var pingTask = PingLoopAsync(linked.Token);

        await Task.WhenAny(receiveTask, sendTask);
        linked.Cancel();

        try { await Task.WhenAll(receiveTask, sendTask, pingTask); }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogDebug(ex, "Task cleanup on disconnect"); }

        if (ws.State == WebSocketState.Open)
        {
            try { await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None); }
            catch { /* best effort */ }
        }

        _sendChannel = null;
        _logger.LogInformation("Disconnected from Remootio");
        ConnectionChanged?.Invoke(false, false);
    }

    public void SendOpen() => SendAction("OPEN");
    public void SendClose() => SendAction("CLOSE");

    private void SendAction(string actionType)
    {
        if (_lastActionId is null || _apiSessionKey is null)
        {
            _logger.LogWarning("Cannot send {Action} — not authenticated", actionType);
            return;
        }

        int nextId = (_lastActionId.Value + 1) % 0x7fffffff;
        string payload = $"{{\"action\":{{\"type\":\"{actionType}\",\"id\":{nextId}}}}}";

        string? frame = RemootioApiCrypto.Encrypt(payload, _config.ApiAuthKey, _apiSessionKey);
        if (frame is not null) Enqueue(frame);
    }

    private void SendQuery()
    {
        if (_lastActionId is null || _apiSessionKey is null) return;

        int nextId = (_lastActionId.Value + 1) % 0x7fffffff;
        string payload = $"{{\"action\":{{\"type\":\"QUERY\",\"id\":{nextId}}}}}";

        string? frame = RemootioApiCrypto.Encrypt(payload, _config.ApiAuthKey, _apiSessionKey);
        if (frame is not null) Enqueue(frame);
    }

    private void Enqueue(string json) => _sendChannel?.Writer.TryWrite(json);

    private async Task SendLoopAsync(ClientWebSocket ws, CancellationToken ct)
    {
        try
        {
            await foreach (var json in _sendChannel!.Reader.ReadAllAsync(ct))
            {
                if (ws.State != WebSocketState.Open) break;
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogError(ex, "WebSocket send error"); }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket ws, CancellationToken ct)
    {
        var buffer = new byte[65536];
        try
        {
            while (ws.State == WebSocketState.Open)
            {
                int offset = 0;
                ValueWebSocketReceiveResult result;
                do
                {
                    result = await ws.ReceiveAsync(buffer.AsMemory(offset), ct);
                    if (result.MessageType == WebSocketMessageType.Close) return;
                    offset += result.Count;
                }
                while (!result.EndOfMessage);

                string json = Encoding.UTF8.GetString(buffer, 0, offset);
                ProcessMessage(json);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { _logger.LogError(ex, "WebSocket receive error"); }
    }

    private async Task PingLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_config.SendPingMessageEveryXMs));
        try
        {
            while (await timer.WaitForNextTickAsync(ct))
                Enqueue("{\"type\":\"PING\"}");
        }
        catch (OperationCanceledException) { }
    }

    private void ProcessMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (!root.TryGetProperty("type", out var typeEl)) return;

            switch (typeEl.GetString())
            {
                case "PONG":
                    break;

                case "ENCRYPTED":
                    ProcessEncryptedFrame(root);
                    break;

                case "ERROR":
                    var msg = root.TryGetProperty("errorMessage", out var err) ? err.GetString() : "unknown";
                    _logger.LogWarning("Remootio device error: {Error}", msg);
                    break;

                default:
                    _logger.LogDebug("Received frame: {Type}", typeEl.GetString());
                    break;
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to process message"); }
    }

    private void ProcessEncryptedFrame(JsonElement root)
    {
        var dataEl = root.GetProperty("data");
        string rawDataJson = dataEl.GetRawText();   // preserve original bytes for MAC check
        string iv = dataEl.GetProperty("iv").GetString()!;
        string payload = dataEl.GetProperty("payload").GetString()!;
        string mac = root.GetProperty("mac").GetString()!;

        string? decrypted = RemootioApiCrypto.Decrypt(
            rawDataJson, iv, payload, mac,
            _config.ApiSecretKey, _config.ApiAuthKey, _apiSessionKey);

        if (decrypted is null)
        {
            _logger.LogWarning("Failed to decrypt Remootio frame (bad MAC or key)");
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(decrypted);
            var inner = doc.RootElement;

            if (inner.TryGetProperty("challenge", out var challenge))
            {
                _apiSessionKey = challenge.GetProperty("sessionKey").GetString();
                _lastActionId = challenge.GetProperty("initialActionId").GetInt32();
                _waitingForAuthQueryResponse = true;
                _logger.LogDebug("Auth challenge received — sending QUERY");
                SendQuery();
            }
            else if (inner.TryGetProperty("response", out var response))
            {
                if (response.TryGetProperty("id", out var idEl))
                {
                    int id = idEl.GetInt32();
                    if (_lastActionId.HasValue &&
                        (id > _lastActionId || (id == 0 && _lastActionId == 0x7fffffff)))
                        _lastActionId = id;
                }

                if (response.GetProperty("type").GetString() == "QUERY" && _waitingForAuthQueryResponse)
                {
                    _waitingForAuthQueryResponse = false;
                    _logger.LogInformation("Remootio authenticated");
                    ConnectionChanged?.Invoke(true, true);
                }

                if (response.TryGetProperty("state", out var stateEl))
                    ProcessSensorState(stateEl.GetString());
            }
            else if (inner.TryGetProperty("event", out var evt))
            {
                if (evt.GetProperty("type").GetString() == "StateChange" &&
                    evt.TryGetProperty("state", out var stateEl))
                    ProcessSensorState(stateEl.GetString());
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "Failed to parse decrypted frame"); }
    }

    private void ProcessSensorState(string? sensorState)
    {
        if (sensorState is null) return;
        GateStateReceived?.Invoke(new GateState { IsOpen = sensorState == "open" });
    }
}
