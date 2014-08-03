namespace Castle.Facilities.Zmq.Rpc.Internal
{
	using Castle.Facilities.Zmq.Rpc.Remote;
	using Castle.Zmq;
	using Castle.Zmq.Extensions;

	internal class ZmqRpcCleaner
	{
		private readonly IZmqContext _context;

		public ZmqRpcCleaner(IZmqContext context)
		{
			_context = context;
		}

		public RemoteRequestListener Listener { get; set; }

		public RequestPoll RequestPoll { get; set; }

		public void CleanUp()
		{
			if (this.Listener != null)
			{
				this.Listener.Stop();
			}

			if (this.RequestPoll != null)
			{
				this.RequestPoll.Dispose();
			}

			_context.Dispose();
		}
	}
}