namespace Castle.Zmq.Extensions
{
	using System;
	using System.Runtime.ExceptionServices;
	using System.Security;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	/// Uses zmq_proxy to dispatch messages back and forward 
	/// between a frontend socket and a backend proxy. 
	/// See derived type for patterns implemented with it
	/// </summary>
	public abstract class Device : IDisposable
	{
		private readonly IZmqContext _ctx;
		private readonly SocketType _frontendType;
		private readonly SocketType _backendType;
		private readonly bool _enableCapture;
		private volatile bool _disposed;
		private readonly bool _ownSockets;
		private readonly bool _needsBinding;

		public event Action<byte[]> Captured;

		protected Device(IZmqSocket frontend, IZmqSocket backend, bool enableCapture = false)
		{
			if (frontend == null) throw new ArgumentNullException("frontend");
			if (backend == null) throw new ArgumentNullException("backend");

			this.Frontend = frontend;
			this.Backend = backend;

			this._needsBinding = false;
			this._enableCapture = enableCapture;
		}

		protected Device(IZmqContext ctx, string frontEndEndpoint, string backendEndpoint,
						 SocketType frontendType, SocketType backendType, bool enableCapture = false)
		{
			if (ctx == null) throw new ArgumentNullException("ctx");
			if (string.IsNullOrEmpty(frontEndEndpoint)) throw new ArgumentNullException("frontEndEndpoint");
			if (string.IsNullOrEmpty(backendEndpoint)) throw new ArgumentNullException("backendEndpoint");

			this._ctx = ctx;
			this._frontendType = frontendType;
			this._backendType = backendType;
			this._enableCapture = enableCapture;
			this._ownSockets = true;
			this._needsBinding = true;

			this.FrontEndEndpoint = frontEndEndpoint;
			this.BackendEndpoint = backendEndpoint;
		}

		public string FrontEndEndpoint { get; private set; }
		public string BackendEndpoint { get; private set; }

		public IZmqSocket Frontend { get; private set; }
		public IZmqSocket Backend { get; private set; }

		~Device()
		{
			InternalDispose(false);
		}

		[HandleProcessCorruptedStateExceptions, SecurityCritical]
		public virtual void Start()
		{
			EnsureNotDisposed();
			if (!this._ownSockets)
			{
				if (!(this.Frontend is Socket)) throw new InvalidOperationException("Frontend instance is not a Socket");
				if (!(this.Backend is Socket)) throw new InvalidOperationException("Backend instance is not a Socket");
			}

			var thread = new Thread(() =>
			{
				if (this._ownSockets)
				{
					this.Frontend = _ctx.CreateSocket(this._frontendType);
					this.Backend = _ctx.CreateSocket(this._backendType);
				}

				var front = (Socket)this.Frontend;
				var back = (Socket)this.Backend;

				StartFrontEnd();
				StartBackEnd();

				IZmqSocket capReceiver;
				IZmqSocket captureSink = capReceiver = null;

				if (this._enableCapture)
				{
					var rnd = new Random((int)DateTime.Now.Ticks);
					
					var captureendpoint = "inproc://capture" + rnd.Next(0, Int32.MaxValue);
					captureSink = _ctx.Pair();
					captureSink.Bind(captureendpoint);

					capReceiver = _ctx.Pair();
					capReceiver.Connect(captureendpoint);

					var captureThread = new Thread(() =>
					{
						try
						{
							while (true)
							{
								var data = capReceiver.Recv();
								if (data == null) continue;

								var ev = this.Captured;
								if (ev != null)
								{
									ev(data);
								}
							}
						}
						catch (Exception e)
						{
							if (LogAdapter.LogEnabled)
							{
								LogAdapter.LogError("DeviceCapture", e.ToString());
							}
						}
					})
					{
						IsBackground = true, 
						Name = "Capture thread for " + captureendpoint
					};
					captureThread.Start();
				}

				var captureHandle = _enableCapture ? captureSink.Handle() : IntPtr.Zero;

			restart:
				// this will block forever, hence it's running in a separate thread
				var res = Native.Device.zmq_proxy(front.Handle(), back.Handle(), captureHandle);
				if (res == Native.ErrorCode)
				{
					if (Native.LastError() == ZmqErrorCode.EINTR) // unix interruption
					{
						goto restart;
					}

					// force disposal since these sockets were eterm'ed or worse
					this.Dispose();

					if (captureSink != null) captureSink.Dispose();
					if (capReceiver != null) capReceiver.Dispose();

					// this is expected
					if (Native.LastError() == ZmqErrorCode.ETERM) return;
					
					// not expected
					var msg = "Error on zmq_proxy: " + Native.LastErrorString();
					System.Diagnostics.Trace.TraceError(msg);
					System.Diagnostics.Debug.WriteLine(msg);
					if (LogAdapter.LogEnabled)
					{
						LogAdapter.LogError(this.GetType().FullName, msg);
					}
				}
			})
			{
				IsBackground = true
			};
			thread.Start();
		}

		protected virtual void StartFrontEnd()
		{
			if (this._needsBinding)
			{
				this.Frontend.Bind(this.FrontEndEndpoint);
			}
		}
		protected virtual void StartBackEnd()
		{
			if (this._needsBinding)
			{
				this.Backend.Bind(this.BackendEndpoint);
			}
		}

		protected virtual void DoDispose()
		{
		}

		public void Dispose()
		{
			this.InternalDispose(true);
		}

		protected void InternalDispose(bool isDispose)
		{
			if (this._disposed) return;

			if (isDispose)
			{
				GC.SuppressFinalize(this);
			}

			this._disposed = true;

			if (_ownSockets)
			{
				if (this.Frontend != null) this.Frontend.Dispose();
				if (this.Backend != null) this.Backend.Dispose();
			}
		}

		internal void EnsureNotDisposed()
		{
			if (_disposed) throw new ObjectDisposedException("Device was disposed");
		}
	}
}
