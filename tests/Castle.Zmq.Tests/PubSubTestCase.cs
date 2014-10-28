namespace Castle.Zmq.Tests
{
	using System.Threading;
	using NUnit.Framework;

	[TestFixture]
	public class PubSubTestCase : BaseContextSetUp
	{
		[Test]
		public void creating_pub()
		{
			using (var pubSocket = base.Context.CreateSocket(SocketType.Pub))
			{
				pubSocket.Bind("tcp://0.0.0.0:90010");

				using (var subSocket = base.Context.CreateSocket(SocketType.Sub))
				{
					subSocket.SetOption(SocketOpt.RCVTIMEO, 100);
					subSocket.Connect("tcp://127.0.0.1:90010");

					pubSocket.Send("topic", null, hasMoreToSend: true);

					// this will NOT work since it did not subscribe
					var msg = subSocket.RecvString();
					Assert.IsNull(msg);
				}
			}
		}

		[Test]
		public void creating_sub_and_subscribing_to_all()
		{
			using (var pubSocket = base.Context.CreateSocket(SocketType.Pub))
			{
				pubSocket.Bind("tcp://0.0.0.0:90011");

				using (var subSocket = base.Context.CreateSocket(SocketType.Sub))
				{
					subSocket.SetOption(SocketOpt.RCVTIMEO, 100);
					subSocket.Connect("tcp://127.0.0.1:90011");
					subSocket.SubscribeAll();

					Thread.Sleep(100);

					pubSocket.Send("topic", null, hasMoreToSend: true);
					pubSocket.Send("data");

					var topic = subSocket.RecvString();
					Assert.IsNotNull(topic);
					Assert.IsTrue(subSocket.HasMoreToRecv());

					var data = subSocket.RecvString();
					Assert.IsNotNull(data);
					Assert.AreEqual("topic", topic);
					Assert.AreEqual("data", data);
				}
			}
		}

		[Test]
		public void creating_sub_and_subscribing_to_specific_topic()
		{
			using (var pubSocket = base.Context.CreateSocket(SocketType.Pub))
			{
				pubSocket.Bind("tcp://0.0.0.0:90012");

				using (var subSocket = base.Context.CreateSocket(SocketType.Sub))
				{
					subSocket.SetOption(SocketOpt.RCVTIMEO, 100);
					subSocket.Connect("tcp://127.0.0.1:90012");
					subSocket.Subscribe("topic");

					Thread.Sleep(100);

					pubSocket.Send("topic", null, hasMoreToSend: true);
					pubSocket.Send("data");

					var topic = subSocket.RecvString();
					Assert.IsNotNull(topic);
					Assert.IsTrue(subSocket.HasMoreToRecv());

					var data = subSocket.RecvString();
					Assert.IsNotNull(data);
					Assert.AreEqual("topic", topic);
					Assert.AreEqual("data", data);
				}
			}
		}

		[Test]
		public void creating_sub_and_subscribing_to_specific_weird_topic()
		{
			using (var pubSocket = base.Context.CreateSocket(SocketType.Pub))
			{
				pubSocket.Bind("tcp://0.0.0.0:90012");

				using (var subSocket = base.Context.CreateSocket(SocketType.Sub))
				{
					subSocket.SetOption(SocketOpt.RCVTIMEO, 100);
					subSocket.Connect("tcp://127.0.0.1:90012");
					subSocket.Subscribe("topicX");

					Thread.Sleep(100);

					pubSocket.Send("topic", null, hasMoreToSend: true);
					pubSocket.Send("data");

					Thread.Sleep(100);

					var topic = subSocket.RecvString();
					Assert.IsNull(topic);
				}
			}
		}

		[Test]
		public void receive_nowait_blocking()
		{
			using (var pubSocket = base.Context.CreateSocket(SocketType.Pub))
			{
				pubSocket.Bind("tcp://0.0.0.0:90012");

				using (var subSocket = base.Context.CreateSocket(SocketType.Sub))
				{
					subSocket.SetOption(SocketOpt.RCVTIMEO, 100);
					subSocket.Connect("tcp://127.0.0.1:90012");
					subSocket.Subscribe("topicX");

					Thread.Sleep(100);

					pubSocket.Send("topic", null, hasMoreToSend: true);
					pubSocket.Send("data");

					Thread.Sleep(100);

					var topic = subSocket.RecvString();
					Assert.IsNull(topic);
				}
			}
		}
	}
}