namespace Castle.Facilities.Zmq.Rpc.Internal
{
	using Castle.Zmq;

	internal class ZmqRpcCleaner
	{
		private readonly IZmqContext _context;

		public ZmqRpcCleaner(IZmqContext context)
		{
			_context = context;
		}

		public void CleanUp()
		{
			_context.Dispose();
		}
	}
}