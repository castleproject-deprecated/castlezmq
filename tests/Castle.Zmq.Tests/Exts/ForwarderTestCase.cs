namespace Castle.Zmq.Tests.Exts
{
	using System.Threading;
	using Castle.Zmq.Extensions;
	using NUnit.Framework;

	[TestFixture]
	public class ForwarderTestCase : BaseContextSetUp
	{
		[Test]
		public void forwarder_for_pub()
		{
			const string pubEndpoint  = "tcp://127.0.0.1:90012";
			const string XpubEndpoint = "tcp://127.0.0.1:80010";

			// set up a pub sub

			using (var pubSocket = base.Context.CreateSocket(SocketType.Pub))
			using (var subSocket = base.Context.CreateSocket(SocketType.Sub))
			{
				pubSocket.Bind(pubEndpoint);

				// this is an indirection between the original pub/sub using xsub/xpub
				using (var fwd = new Forwarder(base.Context, pubEndpoint, XpubEndpoint))
				{
					fwd.Start();

					// connects to xpub
					subSocket.Connect(XpubEndpoint);
					subSocket.Subscribe("topic");

					Thread.Sleep(1000);

					pubSocket.Send("topic", null, hasMoreToSend: true);
					pubSocket.Send("data");

					Thread.Sleep(1000);

					var topic = subSocket.RecvString();
					Assert.AreEqual("topic", topic);

					var data = subSocket.RecvString();
					Assert.AreEqual("data", data);
				}
			}
		}
	}
}
