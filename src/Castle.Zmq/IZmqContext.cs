namespace Castle.Zmq
{
	public interface IZmqContext
	{
		IZmqSocket CreateSocket(SocketType type);
	}
}