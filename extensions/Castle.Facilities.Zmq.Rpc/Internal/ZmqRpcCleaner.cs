namespace Castle.Facilities.Zmq.Rpc.Internal
{
	using Castle.Facilities.Zmq.Rpc.Remote;
	using Castle.Zmq;

	internal class ZmqRpcCleaner
	{
		private readonly IZmqContext _context;

		public ZmqRpcCleaner(IZmqContext context)
		{
			_context = context;
		}

		public RemoteRequestListener Listener { get; set; }

		public void CleanUp()
		{
			if (this.Listener != null)
			{
				this.Listener.Stop();
			}

			_context.Dispose();
		}
	}
}