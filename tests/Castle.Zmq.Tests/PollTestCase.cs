namespace Castle.Zmq.Tests
{
	using NUnit.Framework;

	[TestFixture]
	public class PollTestCase : BaseContextSetUp
	{
		[Test]
		public void poll_in_for_req_rep()
		{
			using (var repSocket = base.Context.CreateSocket(SocketType.Rep))
			using (var reqSocket = base.Context.CreateSocket(SocketType.Req))
			{
				repSocket.Bind("tcp://0.0.0.0:90001");

				var polling = new Polling(PollingEvents.RecvReady, repSocket, reqSocket);

				var rcEvCalled = false;

				polling.RecvReady += (socket) =>
				{
					rcEvCalled = true;
				};
				
				reqSocket.Connect("tcp://127.0.0.1:90001");
				reqSocket.Send("Hello");
				
				polling.PollForever();

				Assert.IsTrue(rcEvCalled);
			}
		}
	}
}