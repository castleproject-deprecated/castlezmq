namespace Castle.Zmq
{
	using System;
	using System.Threading;

	/// <summary>
	/// Simplify monitoring by exposing just simple events: connected/disconnected/error
	/// </summary>
	public class MonitoredSocket : IDisposable
	{
		private readonly IZmqSocket _socket;
		private readonly Monitor _monitor;

		private volatile bool _inError, _disconnected, _connected;

		public event Action<IZmqSocket, MonitorEvents, ZmqError> Error;
		public event Action<IZmqSocket, MonitorEvents, string> Connected;
		public event Action<IZmqSocket, MonitorEvents, string> Disconnected;


		public MonitoredSocket(IZmqContext context, IZmqSocket socket)
		{
			this._socket = socket;

			this._monitor = new Castle.Zmq.Monitor(socket, context);
			this._monitor.SocketEvent += (sender, args) =>
			{
				switch (args.Event)
				{
					case MonitorEvents.BindFailed:
					case MonitorEvents.AcceptFailed:
					case MonitorEvents.CloseFailed:
						if (!_inError)
						{
							_inError = true;
							Thread.MemoryBarrier();

							FireError(args);
						}
						break;
					case MonitorEvents.ConnectRetried:
					case MonitorEvents.Closed:
					case MonitorEvents.Disconnected:
						if (!_disconnected)
						{
							_disconnected = true;
							_connected = false;
							Thread.MemoryBarrier();

							FireDisconnected(args);
						}
						break;
					case MonitorEvents.Connected:
					case MonitorEvents.Listening:
						if (!_connected)
						{
							_connected = true;
							_inError = _disconnected = false;
							Thread.MemoryBarrier();

							FireConnected(args);
						}
						break;
				}
			};
		}

		~MonitoredSocket()
		{
			this.Dispose();
		}

		public void Dispose()
		{
			_monitor.Dispose();
		}

		private void FireConnected(MonitorEventArgs args)
		{
			var ev = this.Connected;
			if (ev != null)
			{
				ev(this._socket, args.Event, args.Endpoint);
			}
		}
		private void FireDisconnected(MonitorEventArgs args)
		{
			var ev = this.Disconnected;
			if (ev != null)
			{
				ev(this._socket, args.Event, args.Endpoint);
			}
		}
		private void FireError(MonitorEventArgs args)
		{
			var ev = this.Error;
			if (ev != null)
			{
				ev(this._socket, args.Event, args.Error);
			}
		}
	}
}