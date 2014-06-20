namespace Castle.Zmq
{
	using System;

	/// <summary>
	/// This is only exposed to aid libraries in 
	/// stubing the real implementations
	/// </summary>
	public interface IZmqSocket : IDisposable
	{
		void Bind(string endpoint);
		void Unbind(string endpoint);
		void Connect(string endpoint);
		void Disconnect(string endpoint);

		byte[] Recv();

		void Send(byte[] buffer, bool hasMoreToSend = false, bool noWait = false);

		void Subscribe(string topic);

		void Unsubscribe(string topic);

		void SetOption(int option, IntPtr value, int valueSize);
		T GetOption<T>(int option);
	}
}