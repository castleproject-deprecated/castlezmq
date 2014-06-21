namespace Castle.Zmq
{

	/// <summary>
	/// Convinience API
	/// </summary>
	public static class ContextExtensions
	{
		public static IZmqSocket Pub(this Context source)
		{
			return source.CreateSocket(SocketType.Pub);
		}
		public static IZmqSocket Sub(this Context source)
		{
			return source.CreateSocket(SocketType.Sub);
		}
		public static IZmqSocket Router(this Context source)
		{
			return source.CreateSocket(SocketType.Router);
		}
		public static IZmqSocket Dealer(this Context source)
		{
			return source.CreateSocket(SocketType.Dealer);
		}
		public static IZmqSocket XPub(this Context source)
		{
			return source.CreateSocket(SocketType.XPub);
		}
		public static IZmqSocket XSub(this Context source)
		{
			return source.CreateSocket(SocketType.XSub);
		}
		public static IZmqSocket Pull(this Context source)
		{
			return source.CreateSocket(SocketType.Pull);
		}
		public static IZmqSocket Push(this Context source)
		{
			return source.CreateSocket(SocketType.Push);
		}
		public static IZmqSocket Rep(this Context source)
		{
			return source.CreateSocket(SocketType.Rep);
		}
		public static IZmqSocket Req(this Context source)
		{
			return source.CreateSocket(SocketType.Req);
		}
	}
}