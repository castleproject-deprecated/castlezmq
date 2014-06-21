namespace Castle.Zmq
{
	using System;

	/// <summary>
	/// This is only exposed to aid libraries in 
	/// stubing/mocking the real implementation. 
	/// 
	/// This api is kept at minimum. 
	/// A more friendly API is exposed by the extensions on SocketExtensions
	/// </summary>
	public interface IZmqSocket : IDisposable
	{
		SocketType SocketType { get; }

		void Bind(string endpoint);
		void Unbind(string endpoint);
		void Connect(string endpoint);
		void Disconnect(string endpoint);

		byte[] Recv(int flags = 0);

		void Send(byte[] buffer, bool hasMoreToSend = false, bool noWait = false);

		void Subscribe(string topic);

		void Unsubscribe(string topic);

		void SetOption<T>(int option, T value);
		T GetOption<T>(int option);
	}
}