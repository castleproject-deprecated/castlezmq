namespace Castle.Zmq.Tests.Exts
{
	using Castle.Zmq.Extensions;
	using NUnit.Framework;

	[TestFixture]
	public class SharedQueueTestCase : BaseContextSetUp
	{
		[Test]
		public void tcp_frontend_inproc_backend()
		{
			const string FrontEndpoint = "tcp://127.0.0.1:10001";
			const string BackEndpoint = "inproc://sharedq1";

			using (var queue = new SharedQueue(base.Context, FrontEndpoint, BackEndpoint))
			{
				queue.Start();
				
				// to test it, we need to send something to the front, and get it at the back

				using (var request = base.Context.Req())
				using (var reply = base.Context.Rep())
				{
					reply.Bind(BackEndpoint);

					request.Connect(FrontEndpoint);

					const string reqData = "Can I get a grande, non-fat, no whip Mocha, please?";
					request.Send(reqData);

					var rcData = reply.RecvString();
					Assert.AreEqual(reqData, rcData);

					const string replyData = "Here's your drink";
					reply.Send(replyData);

					var result = request.RecvString();
					Assert.AreEqual(replyData, result);
				}
			}
		}
	}
}
