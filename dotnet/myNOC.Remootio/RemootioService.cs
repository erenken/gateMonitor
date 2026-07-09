using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace myNOC.Remootio;

/// <summary>
/// High-level Remootio service. Manages device connection lifecycle and exposes gate state
/// to consumers via events. Register as both IHostedService and IRemootioService so the
/// same singleton handles startup/shutdown and UI subscriptions:
/// <code>
/// services.AddSingleton&lt;RemootioService&gt;();
/// services.AddSingleton&lt;IRemootioService&gt;(sp =&gt; sp.GetRequiredService&lt;RemootioService&gt;());
/// services.AddHostedService(sp =&gt; sp.GetRequiredService&lt;RemootioService&gt;());
/// </code>
/// </summary>
public sealed class RemootioService : IHostedService, IRemootioService
{
    private readonly RemootioDeviceConfig _config;
    private readonly ILogger<RemootioService> _logger;
    private RemootioDevice? _device;
    private CancellationTokenSource? _cts;

    public GateState? CurrentGateState { get; private set; }
    public bool IsAuthenticated { get; private set; }

    public event EventHandler<GateState>? GateStateChanged;
    public event EventHandler<bool>? ConnectionChanged;

    public RemootioService(IOptions<RemootioDeviceConfig> config, ILogger<RemootioService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _device = new RemootioDevice(_config, _logger);
        _device.GateStateReceived += OnGateStateReceived;
        _device.ConnectionChanged += OnConnectionChanged;

        _ = RunConnectionLoopAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        return Task.CompletedTask;
    }

    public void OpenGate() => _device?.SendOpen();
    public void CloseGate() => _device?.SendClose();

    private async Task RunConnectionLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _device!.RunAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error from Remootio device");
            }

            if (!ct.IsCancellationRequested && _config.AutoReconnect)
            {
                _logger.LogInformation("Reconnecting to Remootio in 5 seconds...");
                try { await Task.Delay(5000, ct); }
                catch (OperationCanceledException) { break; }
            }
            else break;
        }
    }

    private void OnGateStateReceived(GateState state)
    {
        CurrentGateState = state;
        GateStateChanged?.Invoke(this, state);
    }

    private void OnConnectionChanged(bool connected, bool authenticated)
    {
        IsAuthenticated = authenticated;
        ConnectionChanged?.Invoke(this, connected);
    }
}
