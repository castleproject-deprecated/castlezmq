namespace Castle.Zmq.Stubs
{
	using System;
	using System.Collections;
	using System.Collections.Generic;

	/// <summary>
	/// Suggested (base) implementation for Stub
	/// </summary>
	public class StubSocket : IZmqSocket
 	{
		private bool _disposed;

		public StubSocket(SocketType type)
		{
			this.SocketType = type;

			this.BytesSent = new List<byte[]>();
			this.ToRecv = new List<byte[]>();
			this.Subscriptions = new List<string>();
			this.OptionsSet = new Dictionary<SocketOpt, object>();
		}

		// The following api is for assertions / stubbing behavior

		/// <summary>
		/// Every <see cref="Send"/> operation records the data 
		/// sent here for your assertions
		/// </summary>
		public IList<byte[]> BytesSent { get; private set; }

		/// <summary>
		/// For your code to record the buffer you expect 
		/// the <see cref="Recv"/> to return
		/// </summary>
		public IList<byte[]> ToRecv { get; private set; }

		public bool Bound { get; private set; }

		/// <summary>
		/// In theory you can Bind the same socket multiple times. 
		/// We're recording the last one
		/// </summary>
		public string BoundToEndpoint { get; private set; }

		public bool Connected { get; private set; }

		/// <summary>
		/// In theory you can connect the same socket multiple times. 
		/// We're recording the last one
		/// </summary>
		public string ConnectedToEndpoint { get; private set; }

		/// <summary>
		/// Subscriptions recorded. Unsubcribe will remove the topics from here
		/// </summary>
		public IList<string> Subscriptions { get; private set; }

		/// <summary>
		/// Record options set in this socket
		/// </summary>
		public IDictionary<SocketOpt, object> OptionsSet { get; set; }

		#region IZmqSocket implementation

		public SocketType SocketType { get; private set; }

		public virtual void Bind(string endpoint)
		{
			EnsureNotDisposed();
			if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

			this.Bound = true;
			this.BoundToEndpoint = endpoint;
		}

		public virtual void Unbind(string endpoint)
		{
			EnsureNotDisposed();

			this.Bound = false;
			this.BoundToEndpoint = endpoint;
		}

		public virtual void Connect(string endpoint)
		{
			EnsureNotDisposed();

			this.Connected = true;
			this.ConnectedToEndpoint = endpoint;
		}

		public virtual void Disconnect(string endpoint)
		{
			EnsureNotDisposed();

			this.Connected = false;
			this.ConnectedToEndpoint = endpoint;
		}

		public virtual byte[] Recv(int flags = 0)
		{
			EnsureNotDisposed();

			if (this.ToRecv.Count != 0)
			{
				// FIFO
				var buffer = this.ToRecv[0];
				this.ToRecv.RemoveAt(0);

				return buffer;
			}

			// Does not block, as opposed to real one
			return null;
		}

		public virtual void Send(byte[] buffer, bool hasMoreToSend = false, bool noWait = false)
		{
			EnsureNotDisposed();
			if (buffer == null) throw new ArgumentNullException("buffer");

			this.BytesSent.Add(buffer);
		}

		public virtual void Subscribe(string topic)
		{
			EnsureNotDisposed();

			this.Subscriptions.Add(topic);
		}

		public virtual void Unsubscribe(string topic)
		{
			EnsureNotDisposed();

			var index = this.Subscriptions.IndexOf(topic);
			if (index != -1)
			{
				this.Subscriptions.RemoveAt(index);
			}
		}

		public virtual void SetOption<T>(int option, T value)
		{
			EnsureNotDisposed();

			this.OptionsSet[(SocketOpt) option] = value;
		}

		public virtual T GetOption<T>(int option)
		{
			EnsureNotDisposed();

			object value;
			if (OptionsSet.TryGetValue((SocketOpt) option, out value))
			{
				return (T) value;
			}
			return default(T);
		}

		#endregion

		#region IDisposable

		public virtual void Dispose()
		{
			this._disposed = true;
		}

		#endregion


		/// <summary>
		/// This is provided to mimick the behavior 
		/// of the real socket implementation, as if your tests 
		/// operate in a disposed real socket it would 
		/// be indication of a bug.
		/// </summary>
		private void EnsureNotDisposed()
		{
			if (this._disposed) throw new ObjectDisposedException("StubSocket was disposed");
 		}
 	}
}
