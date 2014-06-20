namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;
	using System.Runtime.Remoting.Services;
	using System.Text;


	public class Socket : IZmqSocket, IDisposable
	{
		private readonly SocketType _type;
		private volatile bool _disposed;

		private IntPtr _socketPtr;

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

		internal IntPtr SocketPtr { get { return _socketPtr; } }

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

		public T GetOption<T>(int option)
		{
			EnsureNotDisposed();

			var retT = typeof(T);

			return (T) this.InternalGetOption(retT, option);
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

//		public int RecvInto(byte[] buffer, int flags)
//		{
//			// this one should use zmq_recv
//		}

//		public byte[] RecvAll()
//		{
//			EnsureNotDisposed();
//
//			return null;
//		}

		public byte[] Recv(int flags = 0)
		{
			EnsureNotDisposed();

			using (var frame = new MsgFrame())
			{
				var res = Native.MsgFrame.zmq_msg_recv(frame._msgPtr, this._socketPtr, flags);

				if (res == Native.ErrorCode)
				{
					var error = Native.LastError();
					if (error == Native.Socket.EAGAIN)
					{
						// not the end of the world
						return null;
					}
					Native.ThrowZmqError(error);
				}
				else
				{
					return frame.ToBytes();
				}
			}

			return null;
		}

		public void Send(byte[] buffer, bool hasMoreToSend = false, bool noWait = false)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			EnsureNotDisposed();

			var flags = hasMoreToSend ? Native.Socket.SNDMORE : 0;
			flags += noWait ? Native.Socket.DONTWAIT : 0; 

			var len = buffer.Length;

			var res = Native.Socket.zmq_send(this._socketPtr, buffer, len, flags);

			// for now we're treating EAGAIN as error. 
			// not sure that's OK
			if (res == Native.ErrorCode) Native.ThrowZmqError();
		}

		/// <summary>
		/// The SUBSCRIBE option shall establish a new message filter on a ZMQ_SUB socket. 
		/// Newly created ZMQ_SUB sockets shall filter out all incoming messages, therefore 
		/// you should call this option to establish an initial message filter.
		/// </summary>
		/// <param name="topic">topic name</param>
		public void Subscribe(string topic)
		{
			if (topic == null) throw new ArgumentNullException("topic");
			// should we assert socketType = ZMQ_SUB ?
			EnsureNotDisposed();

			var buf = Encoding.UTF8.GetBytes(topic);
			SocketExtensions.SetOption(this, SocketOpt.SUBSCRIBE, buf);
		}

		/// <summary>
		/// The ZMQ_UNSUBSCRIBE option shall remove an existing message filter on a ZMQ_SUB socket. 
		/// The filter specified must match an existing filter previously established with the 
		/// ZMQ_SUBSCRIBE option. If the socket has several instances of the same filter attached 
		/// the ZMQ_UNSUBSCRIBE option shall remove only one instance, leaving the rest in place and functional.
		/// </summary>
		/// <param name="topic">topic name</param>
		public void Unsubscribe(string topic)
		{
			if (topic == null) throw new ArgumentNullException("topic");
			// should we assert socketType = ZMQ_SUB ?
			EnsureNotDisposed();

			var buf = Encoding.UTF8.GetBytes(topic);
			SocketExtensions.SetOption(this, SocketOpt.UNSUBSCRIBE, buf);
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
			if (res == Native.ErrorCode)
			{
				// we cannot throw in dispose. should we log?
				System.Diagnostics.Debug.WriteLine("Error disposing socket " + Native.LastError());
			}
		}

		private void EnsureNotDisposed()
		{
			if (_disposed) throw new ObjectDisposedException("Socket was disposed");
		}

		private void InternalSetOption(int option, int valueSize, IntPtr value, bool ignoreError)
		{
			// DO NOT check whether it's disposed here

			var res = Native.Socket.zmq_setsockopt(this._socketPtr, option, value, valueSize);
			if (!ignoreError && res == Native.ErrorCode) Native.ThrowZmqError();
		}

		private object InternalGetOption(Type retType, int option)
		{
			Func<IntPtr, Int64, object> unmarshaller;
			Int64 bufferLen;

			BuildUnmarshaller(retType, out unmarshaller, out bufferLen);

			object retValue = null;

			MarshalExt.AllocAndRun(sizePtr =>
			{
				Marshal.WriteInt64(sizePtr, bufferLen);

				MarshalExt.AllocAndRun(bufferPtr =>
				{
					var res = Native.Socket.zmq_getsockopt(this._socketPtr, option, bufferPtr, sizePtr);
					if (res == Native.ErrorCode) Native.ThrowZmqError();

					retValue = unmarshaller(bufferPtr, bufferLen);
				}, bufferLen);

			}, sizeof(Int64));

			return retValue;
		}

		private void BuildUnmarshaller(Type retType, out Func<IntPtr, long, object> unmarshaller, out long bufferLen)
		{
			if (retType == typeof(Int32))
			{
				unmarshaller = (ptr, len) => Marshal.ReadInt32(ptr);
				bufferLen = sizeof(Int32);
			}
			else if (retType == typeof(Int64))
			{
				unmarshaller = (ptr, len) => Marshal.ReadInt64(ptr);
				bufferLen = sizeof(Int64);
			}
			else if (retType == typeof(bool))
			{
				unmarshaller = (ptr, len) => Marshal.ReadInt32(ptr) != 0;
				bufferLen = sizeof(Int32);
			}
			else if (retType == typeof(byte[]))
			{
				unmarshaller = (ptr, len) =>
				{
					var buffer = new byte[len];
					if (len > 0)
						Marshal.Copy(ptr, buffer, 0, (int)len);
					return buffer;
				};
				bufferLen = 255L;
			}
			else if (retType == typeof(string))
			{
				unmarshaller = (ptr, len) =>
				{
					var buffer = new byte[len];
					if (len > 0)
						Marshal.Copy(ptr, buffer, 0, (int)len);
					return Encoding.UTF8.GetString(buffer);
				};
				bufferLen = 255L;
			}
			else
			{
				throw new ArgumentException("Unsupported option type: " + retType.Name);
			}
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