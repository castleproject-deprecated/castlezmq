namespace Castle.Zmq.Extensions
{
	using System;

	/// <summary>
	/// Exposes a xsub and xpub, in order to forward pub messages to sub 
	/// (scenarios: changing protocol, proxies, etc)
	/// 
	/// <para>
	/// When the frontend is a ZMQ_XSUB socket, and the backend is a ZMQ_XPUB socket, 
	/// the proxy shall act as a message forwarder that collects messages from a set of publishers 
	/// and forwards these to a set of subscribers. This may be used to bridge networks 
	/// transports, e.g. read on tcp:// and forward on pgm://.
	/// </para>
	/// </summary>
	public class Forwarder : Device
	{
		public Forwarder(IZmqSocket frontend, IZmqSocket backend) : base(frontend, backend)
		{
			if (frontend.SocketType != SocketType.XSub) throw new ArgumentException("Frontend must be a XSub");
			if (backend.SocketType != SocketType.XPub) throw new ArgumentException("Backend must be a XPub");
		}

		public Forwarder(IZmqContext ctx, string frontEndEndpoint, string backendEndpoint)
			: base(ctx, frontEndEndpoint, backendEndpoint, SocketType.XSub, SocketType.XPub)
		{
		}

		protected override void StartFrontEnd()
		{
			if (this.FrontEndEndpoint != null)
			{
				this.Frontend.Connect(this.FrontEndEndpoint);
			}
		}

		protected override void StartBackEnd()
		{
			if (this.BackendEndpoint != null)
			{
				this.Backend.Bind(this.BackendEndpoint);
			}
		}
	}
}