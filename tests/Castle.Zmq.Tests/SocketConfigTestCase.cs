namespace Castle.Zmq.Tests
{
	using NUnit.Framework;

	[TestFixture]
    public class SocketConfigTestCase
    {
		private Context _context;

		[SetUp]
		public void Init()
		{
			_context = new Context();
		}

		[TearDown]
		public void End()
		{
			if (_context != null) 
				_context.Dispose();
		}

		[Test]
		public void set_int32_sockoption()
		{
			using (var socket = new Socket(_context, SocketType.Req))
			{
				socket.SetOption(SocketOpt.LINGER, 0);
			}
		}

    }
}
