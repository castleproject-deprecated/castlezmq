namespace Castle.Zmq.Extensions
{
	using System;


	public abstract class BaseSubscriber<T> : IDisposable
	{

	}

	public abstract class BasePublisher<T> : IDisposable
	{
		private readonly Func<T, byte[]> _serializer;

		protected BasePublisher(Func<T, byte[]> serializer)
		{
			_serializer = serializer;
		}

		public void Publish(string topic, T message)
		{
			// _socket.Send(topic, hasMore:true);
			// _socket.Send(serialized)
		}

		public virtual void Start()
		{
			
		}
		public virtual void Stop()
		{

		}

		public void Dispose()
		{
			
		}
	}
}
