using System.Net.WebSockets;

namespace myNOC.Remootio
{
	public class Device : IDevice, IAsyncDisposable, IDisposable
	{
		private bool _disposedValue;
		private ClientWebSocket? _clientWebSocket = default;
		private ConnectionStatus _connectionStatus = default!;
		private DeviceConfiguration _configuration = default!;
		private Timer? _pingTimer;

		public ConnectionStatus ConnectionStatus => _connectionStatus;

		public async Task<ConnectionStatus> Connect(DeviceConfiguration configuration)
		{
			await ValidateConfigurationAndCreateClientWebSocket(configuration).ConfigureAwait(false);
			_configuration = configuration;

			if (_clientWebSocket?.State == WebSocketState.Open)
				return _connectionStatus;

			await ConnectToClientWebSocket().ConfigureAwait(false);
			return _connectionStatus;
		}

		private async Task ConnectToClientWebSocket()
		{
			if (_pingTimer != null)
				await _pingTimer.DisposeAsync();

			_connectionStatus = new ConnectionStatus { Connected = false, Authenticated = false };
			var uri = new UriBuilder("ws", _configuration.HostName, 8080).Uri;
			var cancelationToken = new CancellationTokenSource(5000);

			await _clientWebSocket!.ConnectAsync(uri, cancelationToken.Token).ConfigureAwait(false);

			_connectionStatus.Connected = _clientWebSocket.State == WebSocketState.Open;

			if (_configuration.SendPingEveryXMs.HasValue)
				_pingTimer = new Timer(async (_) => await SendPing(), _connectionStatus, _configuration.SendPingEveryXMs.Value, 0);
		}

		private Task SendPing()
		{
			throw new NotImplementedException();
		}

		private async Task ValidateConfigurationAndCreateClientWebSocket(DeviceConfiguration configuration)
		{
			if (string.IsNullOrWhiteSpace(configuration.HostName))
				throw new ArgumentException($"{nameof(configuration.HostName)} is empty.", nameof(configuration));

			if (_configuration?.HostName != configuration.HostName && _clientWebSocket?.State == WebSocketState.Open)
			{
				_connectionStatus.Authenticated = false;
				await _clientWebSocket!.CloseAsync(WebSocketCloseStatus.NormalClosure, "Hostname changed.", CancellationToken.None).ConfigureAwait(false);
				_connectionStatus.Connected = false;
			}

			_clientWebSocket ??= new();
		}

		public Task<ConnectionStatus> Authenticate()
		{
			throw new NotImplementedException();
		}

		#region Dispose
		private protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_pingTimer?.Dispose();

					_clientWebSocket?.Dispose();
					_clientWebSocket = null;
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public async ValueTask DisposeAsync()
		{
			if (_clientWebSocket != null)
			{
				_clientWebSocket.Dispose();
				_clientWebSocket = null;
			}

			if (_pingTimer != null)
				await _pingTimer.DisposeAsync();

			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
