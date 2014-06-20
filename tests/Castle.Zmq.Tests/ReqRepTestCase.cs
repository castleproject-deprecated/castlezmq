namespace Castle.Zmq.Tests
{
	using NUnit.Framework;

	[TestFixture]
	public class ReqRepTestCase : BaseContextSetUp
	{


		[Test]
		public void req_rep()
		{
			using (var reqSocket = base.Context.CreateSocket(SocketType.Req))
			using (var repSocket = base.Context.CreateSocket(SocketType.Rep))
			{
				repSocket.Bind("tcp://0.0.0.0:8801");

				reqSocket.Connect("tcp://0.0.0.0:8801");
				
//				reqSocket.Send();

			}
		}
	}
}
