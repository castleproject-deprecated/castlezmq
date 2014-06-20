namespace Castle.Zmq.Tests
{
	using NUnit.Framework;

	[TestFixture]
    public class SocketConfigTestCase : BaseContextSetUp
    {
		[Test]
		public void set_int32_sockoption_SockType()
		{
			using (var socket = new Socket(base.Context, SocketType.Req))
			{
				var val = socket.GetOption<int>(SocketOpt.TYPE);

				Assert.AreEqual((int)SocketType.Req, val);
			}
		}


		[Test]
		public void set_int32_sockoption_rcvtimeout()
		{
			using (var socket = new Socket(base.Context, SocketType.Req))
			{
				socket.SetOption(SocketOpt.RCVTIMEO, 1000);

				var val = socket.GetOption<int>(SocketOpt.RCVTIMEO);

				Assert.AreEqual(1000, val);
			}
		}
    }
}
