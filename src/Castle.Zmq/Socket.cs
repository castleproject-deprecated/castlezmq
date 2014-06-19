namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;


	public class Socket : IZmqSocket, IDisposable
	{
		private readonly SocketType _type;
		private IntPtr _socketPtr;
		private volatile bool _disposed;

		public const int NoTimeout = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		/// <param name="type"></param>
		/// <param name="rcvTimeoutInMilliseconds"></param>
		public Socket(Context context, SocketType type, int rcvTimeoutInMilliseconds = NoTimeout)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (type < SocketType.Pub || type > SocketType.XSub) throw new ArgumentException("Invalid socket type", "socketType");
			if (rcvTimeoutInMilliseconds < 0) throw new ArgumentException("Invalid rcvTimeout. Must be greater than zero", "rcvTimeoutInMilliseconds");
			if (context.contextPtr == IntPtr.Zero) throw new ArgumentException("Specified context has been disposed", "context");

			this._type = type;
			this._socketPtr = Native.Socket.zmq_socket(context.contextPtr, (int)type);

			if (rcvTimeoutInMilliseconds != NoTimeout)
			{
				// Just in case, to avoid memory leaks
				try
				{
					this.SetOption(SocketOpt.RCVTIMEO, rcvTimeoutInMilliseconds);
				}
				catch (Exception)
				{
					this.InternalDispose(true);
					throw;
				}
			}
		}

		~Socket()
		{
			InternalDispose(false);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Caution: All options, with the exception of 
		/// ZMQ_SUBSCRIBE, ZMQ_UNSUBSCRIBE, ZMQ_LINGER, ZMQ_ROUTER_MANDATORY, ZMQ_PROBE_ROUTER, 
		/// ZMQ_XPUB_VERBOSE, ZMQ_REQ_CORRELATE, and ZMQ_REQ_RELAXED, 
		/// only take effect for subsequent socket bind/connects.
		/// </remarks>
		/// <param name="option"> <see cref="SocketOpt"/> for a list of options </param>
		/// <param name="value"> value must be allocated in unmanaged memory </param>
		/// <param name="valueSize"> size of the block allocated for value </param>
		public void SetOption(int option, IntPtr value, int valueSize)
		{
			EnsureNotDisposed();

			InternalSetOption(option, valueSize, value, ignoreError: false);
		}

		public void Bind(string endpoint)
		{
			if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");
			EnsureNotDisposed();

			var res = Native.Socket.zmq_bind(this._socketPtr, endpoint);
			if (res == Native.ErrorCode) Native.ThrowZmqError();
		}

		public void Unbind(string endpoint)
		{
			if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");
			EnsureNotDisposed();

			var res = Native.Socket.zmq_unbind(this._socketPtr, endpoint);
			if (res == Native.ErrorCode) Native.ThrowZmqError();
		}

		public void Connect(string endpoint)
		{
			if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");
			EnsureNotDisposed();

			var res = Native.Socket.zmq_connect(this._socketPtr, endpoint);
			if (res == Native.ErrorCode) Native.ThrowZmqError();
		}

		public void Disconnect(string endpoint)
		{
			if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");
			EnsureNotDisposed();

			var res = Native.Socket.zmq_disconnect(this._socketPtr, endpoint);
			if (res == Native.ErrorCode) Native.ThrowZmqError();
		}

		public byte[] Recv()
		{
			EnsureNotDisposed();
			return null;
		}

		public void Send(byte[] buffer, bool hasMoreToSend = false)
		{
			EnsureNotDisposed();
		}

		public void Subscribe(string topic)
		{
			EnsureNotDisposed();
			
		}
		public void Unsubscribe(string topic)
		{
			EnsureNotDisposed();

		}

		public void Dispose()
		{
			this.InternalDispose(true);
		}

		private void InternalDispose(bool isDispose)
		{
			if (_disposed) return;

			if (isDispose)
			{
				GC.SuppressFinalize(this);
			}

			_disposed = true;

			TryCancelLinger();

			var res = Native.Socket.zmq_close(this._socketPtr);
			if (res == Native.ErrorCode) Native.ThrowZmqError();
		}

		private void EnsureNotDisposed()
		{
			if (_disposed) throw new ObjectDisposedException("Socket was disposed");
		}

		private void InternalSetOption(int option, int valueSize, IntPtr value, bool ignoreError)
		{
			// Do NOT check for disposed here

			const int len = sizeof(int);

			MarshalExt.AllocAndRun(size =>
			{
				Marshal.WriteInt32(size, valueSize);

				var res = Native.Socket.zmq_setsockopt(this._socketPtr, option, value, size);
				if (!ignoreError && res == Native.ErrorCode) Native.ThrowZmqError();
			}, len);
		}

		private void TryCancelLinger()
		{
			MarshalExt.AllocAndRun((intBuffer) =>
			{
				Marshal.WriteInt32(intBuffer, 0);
				InternalSetOption((int)SocketOpt.LINGER, sizeof(int), intBuffer, ignoreError: true);	
			}, sizeof(int));
		}
	}
}