namespace Castle.Zmq.Extensions
{
	using System;

	public class Forwarder : Device
	{
		public Forwarder(IZmqSocket frontend, IZmqSocket backend) : base(frontend, backend)
		{
			if (frontend.SocketType != SocketType.XSub) throw new ArgumentException("Frontend must be a XSub");
			if (backend.SocketType != SocketType.XPub) throw new ArgumentException("Backend must be a XPub");
		}

		public Forwarder(Context ctx, string frontEndEndpoint, string backendEndpoint)
			: base(ctx, frontEndEndpoint, backendEndpoint, SocketType.XSub, SocketType.XPub)
		{
		}
	}
}