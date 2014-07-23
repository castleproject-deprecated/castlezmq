namespace Castle.Zmq.Extensions
{
	using System;
	using System.Threading;


	public abstract class BaseSubscriber<T> : IDisposable
	{
		private readonly IZmqContext _context;
		private readonly string _endpoint;
		private readonly Func<byte[], T> _deserializer;
		private IZmqSocket _socket;
		private bool _started;
		private volatile bool _disposed;
		private Thread _worker;

		protected BaseSubscriber(IZmqContext context, string endpoint, Func<byte[], T> deserializer)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (endpoint == null) throw new ArgumentNullException("endpoint");
			if (deserializer == null) throw new ArgumentNullException("deserializer");

			this._context = context;
			this._endpoint = endpoint;
			this._deserializer = deserializer;
		}

		protected abstract void OnReceived(string topic, T message);

		public void SubscribeToTopic(string topic)
		{
			if (topic == null) throw new ArgumentNullException("topic");
			if (!_started) throw new InvalidOperationException("Cannot subscribe before starting it");

			_socket.Subscribe(topic);
		}
		public void UnsubscribeFromTopic(string topic)
		{
			if (topic == null) throw new ArgumentNullException("topic");
			if (!_started) throw new InvalidOperationException("Cannot unsubscribe before starting it");

			_socket.Unsubscribe(topic);
		}

		public virtual void Start()
		{
			EnsureNotDisposed();

			if (this._socket == null)
			{
				this._socket = this._context.CreateSocket(SocketType.Sub);
			}
			if (!this._started)
			{
				this._socket.Connect(this._endpoint);

				this._worker = new Thread(OnRecvWorker)
				{
					IsBackground = true
				};
				this._started = true;
				this._worker.Start();
			}
		}

		public virtual void Stop()
		{
			EnsureNotDisposed();
			if (this._started)
			{
				this._socket.Unbind(this._endpoint);
				this._started = false;
			}
		}

		public void Dispose()
		{
			if (_disposed) return;

			try
			{
				this.Stop();
			}
			catch (Exception)
			{
			}

			if (this._socket != null)
			{
				this._socket.Dispose();
			}

			_disposed = true;
		}

		private void OnRecvWorker()
		{
			var polling = new Polling(PollingEvents.RecvReady, this._socket);

			polling.RecvReady += socket =>
			{
				var topic = _socket.RecvString();
				var messageInBytes = _socket.Recv();
				var message = _deserializer(messageInBytes);

				this.OnReceived(topic, message);
			};

			try
			{
				while (_started)
				{
					polling.Poll(1000);
				}
			}
			catch (ZmqException e)
			{
				if (LogAdapter.LogEnabled)
				{
					LogAdapter.LogDebug(this.GetType().FullName, "BaseSubscriber exception. Disposing. Details: " + e.ToString());
				}

				this.Dispose();
			}
		}

		private void EnsureNotDisposed()
		{
			if (_disposed) throw new ObjectDisposedException("Subscriber Disposed");
		}

	}
}
