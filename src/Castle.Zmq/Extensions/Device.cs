namespace Castle.Zmq.Extensions
{
	using System;
	using System.Threading.Tasks;


	/// <summary>
	/// Uses zmq_proxy to dispatch messages back and forward 
	/// between a frontend socket and a backend proxy. 
	/// See derived type for patterns implemented with it
	/// </summary>
	public abstract class Device : IDisposable
	{
		private volatile bool _disposed;
		private readonly bool _ownSockets;
		private readonly bool _initialized;

		protected Device(IZmqSocket frontend, IZmqSocket backend)
		{
			if (frontend == null) throw new ArgumentNullException("frontend");
			if (backend == null) throw new ArgumentNullException("backend");

			this.Frontend = frontend;
			this.Backend = backend;

			this._initialized = true;
		}

		protected Device(Context ctx, string frontEndEndpoint, string backendEndpoint, 
						 SocketType frontendType, SocketType backendType)
		{
			if (ctx == null) throw new ArgumentNullException("ctx");
			if (string.IsNullOrEmpty(frontEndEndpoint)) throw new ArgumentNullException("frontEndEndpoint");
			if (string.IsNullOrEmpty(backendEndpoint)) throw new ArgumentNullException("backendEndpoint");

			this._ownSockets = true;

			this.Frontend = ctx.CreateSocket(frontendType);
			this.Backend = ctx.CreateSocket(backendType);

			try
			{
				this.Frontend.Bind(frontEndEndpoint);
				this.Backend.Connect(backendEndpoint);

				this._initialized = true;
			}
			finally
			{
				this.Dispose();
			}
		}

		public IZmqSocket Frontend { get; private set; }
		public IZmqSocket Backend { get; private set; }

		~Device()
		{
			InternalDispose(false);
		}

		public virtual void Start()
		{
			if (!(this.Frontend is Socket)) throw new InvalidOperationException("Frontend instance is not a Socket");
			if (!(this.Backend is Socket)) throw new InvalidOperationException("Backend instance is not a Socket");

			Task.Factory.StartNew(() =>
			{
				var front = (Socket)this.Frontend;
				var back = (Socket)this.Backend;

				// this will block forever, hence it's running in a separate thread
				var res = Native.Device.zmq_proxy(front.SocketPtr, back.SocketPtr, IntPtr.Zero);
				if (res == Native.ErrorCode) Native.ThrowZmqError();
			});
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

			if (_initialized)
			{
				this.DoDispose();
			}

			if (_ownSockets)
			{
				if (this.Frontend != null) this.Frontend.Dispose();
				if (this.Backend != null) this.Backend.Dispose();
			}
		}
	}
}
