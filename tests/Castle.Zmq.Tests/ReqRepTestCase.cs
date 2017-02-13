namespace Castle.Zmq.Tests
{
	using System.Text;
	using NUnit.Framework;

	[TestFixture]
	public class ReqRepTestCase : BaseContextSetUp
	{
		[Test]
		public void when_recvTimeout_recv_should_return_null()
		{
			using (var repSocket = base.Context.CreateSocket(SocketType.Rep))
			{
				repSocket.SetOption(SocketOpt.RCVTIMEO, 100);

				repSocket.Bind("tcp://0.0.0.0:90001");

				var result = repSocket.Recv();
				Assert.IsNull(result);
			}
		}

		[Test]
		public void req_rep()
		{
			const string MsgReq = "Hello";
			const string MsgReply = "World";

			using (var reqSocket = base.Context.CreateSocket(SocketType.Req))
			using (var repSocket = base.Context.CreateSocket(SocketType.Rep))
			{
				repSocket.Bind("tcp://0.0.0.0:90002");

				reqSocket.Connect("tcp://127.0.0.1:90002");

				reqSocket.Send(MsgReq);

				var msg = repSocket.Recv();
				var msgStr = Encoding.UTF8.GetString(msg);
				Assert.AreEqual(MsgReq, msgStr);

				repSocket.Send(MsgReply);

				msg = reqSocket.Recv();
				msgStr = Encoding.UTF8.GetString(msg);
				Assert.AreEqual(MsgReply, msgStr);
			}
		}

		[Test]
		public void req_rep_do_not_wait()
		{
			const string MsgReq = "Hello";
			const string MsgReply = "World";

			using (var reqSocket = base.Context.CreateSocket(SocketType.Req))
			using (var repSocket = base.Context.CreateSocket(SocketType.Rep))
			{
				repSocket.Bind("tcp://0.0.0.0:90002");

				reqSocket.Connect("tcp://127.0.0.1:90002");

				// This one would hang when there was no message, burning cpu.
				var msgNone = repSocket.Recv(RecvFlags.DoNotWait);
				Assert.IsNull(msgNone);

				reqSocket.Send(MsgReq);

				var msg = repSocket.Recv();
				var msgStr = Encoding.UTF8.GetString(msg);
				Assert.AreEqual(MsgReq, msgStr);

				repSocket.Send(MsgReply);

				msg = reqSocket.Recv();
				msgStr = Encoding.UTF8.GetString(msg);
				Assert.AreEqual(MsgReply, msgStr);
			}
		}
	}
}
