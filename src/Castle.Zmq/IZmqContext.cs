namespace Castle.Zmq
{
	using System;

	public interface IZmqContext : IDisposable
	{
		IZmqSocket CreateSocket(SocketType type);
	}
}