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
			using (var queue = new SharedQueue(base.Context, "tcp://0.0.0.0:10000", "inproc://sharedq1"))
			{
				queue.Start();
				
			}
		}
	}
}
