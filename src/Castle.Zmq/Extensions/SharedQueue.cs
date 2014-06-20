namespace Castle.Zmq.Extensions
{
	using System;

	/// <summary>
	/// Exposes a XREP(ROUTER) and a XREQ(DEALER) to dispatch requests 
	/// to a bunch of workers (connected to xreq)
	/// </summary>
	public class SharedQueue : Device
	{
		public SharedQueue(IZmqSocket frontend, IZmqSocket backend) : base(frontend, backend)
		{
			if (frontend.SocketType != SocketType.Router) throw new ArgumentException("Frontend must be a Router");
			if (backend.SocketType != SocketType.Dealer) throw new ArgumentException("Backend must be a Dealer");
		}

		public SharedQueue(Context ctx, string frontEndEndpoint, string backendEndpoint)
			: base(ctx, frontEndEndpoint, backendEndpoint, SocketType.Router, SocketType.Dealer)
		{
		}
	}
}