namespace Castle.Zmq.Tests
{
	using NUnit.Framework;

	[TestFixture]
    public class SocketConfigTestCase : BaseContextSetUp
    {
		[Test]
		public void set_int32_sockoption()
		{
			using (var socket = new Socket(base.Context, SocketType.Req))
			{
				socket.SetOption(SocketOpt.LINGER, 0);

				var val = socket.GetOption<int>(SocketOpt.LINGER);

				Assert.AreEqual(0, val);
			}
		}

    }
}
