namespace Castle.Zmq
{
	using System;
	using System.Security.Cryptography;
	using System.Threading;

	[Flags]
	public enum MonitorEvents 
	{
		/// <summary>Socket connection established</summary>
		Connected = 1,
		/// <summary>Synchronous connection failed</summary>
		ConnectDelayed = 2,
		/// <summary>Asynchronous (re)connection attempt</summary>
		ConnectRetried = 4,
		/// <summary>Socket bound to address; ready to accept connections</summary>
		Listening = 8,
		/// <summary>Socket could not bind to address</summary>
		BindFailed = 16,
		/// <summary>Connection accepted to bound interface</summary>
		Accepted = 32,
		/// <summary>Could not accept client connection</summary>
		AcceptFailed = 64,
		/// <summary>Socket connection closed</summary>
		Closed = 128,
		/// <summary>Connection could not be closed (only for ipc transport)</summary>
		CloseFailed = 256,
		/// <summary>Broken session (specific to ipc and tcp transports)</summary>
		Disconnected = 512,
		/// <summary>Event monitoring has been disabled</summary>
		MonitorStopped = 1024,

		/// <summary>All events</summary>
		All = 2047
	}

	public class MonitorEventArgs : EventArgs
	{
		public MonitorEvents Event { get; internal set; }
		public string Endpoint { get; internal set; }
		public ZmqError Error { get; internal set; }
	}

	public class Monitor : IDisposable
	{
		private static readonly Random Rnd = new Random((int)DateTime.Now.Ticks);
		
		private readonly string _monitorName;
		private readonly Thread _thread;
		private IZmqSocket _pairSocket;

		private volatile bool _disposed;

		public event EventHandler<MonitorEventArgs> SocketEvent;

		public Monitor(IZmqSocket socket, IZmqContext context, 
					   string monitorName = null, 
					   MonitorEvents events = MonitorEvents.All)
		{
			if (socket == null) throw new ArgumentNullException("socket");
			if (context == null) throw new ArgumentNullException("context");

			if (monitorName != null)
			{
				if (!monitorName.StartsWith("inproc://"))
					monitorName = "inproc://" + monitorName;
			}
			else
			{
				monitorName = "inproc://temp" + Rnd.Next(0, Int32.MaxValue);
			}

			this._monitorName = monitorName;

			// Creates a inproc socket pair
			var res = Native.Monitor.zmq_socket_monitor(socket.Handle(), monitorName, (int)events);
			if (res == Native.ErrorCode)
			{
				Native.ThrowZmqError("Monitor");
			}

			// Connects to the newly created socket pair
			this._pairSocket = context.Pair();
			this._pairSocket.Connect(this._monitorName);

			this._thread = new Thread(EventsWorker)
			{
				IsBackground = true
			};
			this._thread.Start();
		}

		~Monitor()
		{
			InternalDispose(false);
		}

		public void Dispose()
		{
			InternalDispose(true);
		}

		private void EventsWorker()
		{
			try
			{
				while (!_disposed)
				{
					var binary = _pairSocket.Recv();

					if (binary == null) continue;

					var ev = (MonitorEvents) BitConverter.ToUInt16(binary, 0);
					int val = 0;

					if (binary.Length > sizeof (UInt16))
					{
						val = BitConverter.ToInt32(binary, sizeof(UInt16));
					}

					ZmqError error = null;
					var address = "";

					switch (ev)
					{
						case MonitorEvents.BindFailed:
						case MonitorEvents.AcceptFailed:
						case MonitorEvents.CloseFailed:
							address = _pairSocket.RecvString();
							error = new ZmqError(val);
							break;
						case MonitorEvents.MonitorStopped:
							break;
						default:
							address = _pairSocket.RecvString();
							break;
					}

					FireEvent(ev, address, error);
				}
			}
			catch (ZmqException ex)
			{
				if (ex.ZmqErrorCode != ZmqErrorCode.ETERM)
				{
					if (LogAdapter.LogEnabled)
					{
						LogAdapter.LogError("Monitor", ex.ToString());
					}
				}
			}
			catch (Exception e)
			{
				if (LogAdapter.LogEnabled)
				{
					LogAdapter.LogError("Monitor", e.ToString());
				}
			}
			finally
			{
				this.Dispose();
			}
		}

		private void FireEvent(MonitorEvents ev, string endpoint, ZmqError error)
		{
			try
			{
				var _event = this.SocketEvent;

				if (_event != null)
				{
					_event(this, new MonitorEventArgs()
					{
						Event = ev,
						Endpoint = endpoint,
						Error = error
					});
				}
			}
			catch (Exception) { }
		}

		private void InternalDispose(bool isDisposing)
		{
			if (this._disposed) return;

			this._disposed = true;

			if (isDisposing)
			{
				GC.SuppressFinalize(this);
			}

			if (this._pairSocket != null)
			{
				this._pairSocket.Dispose();
			}
		}
	}
}
