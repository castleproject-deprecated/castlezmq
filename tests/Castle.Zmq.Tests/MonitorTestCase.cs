namespace Castle.Zmq.Tests
{
	using System;
	using NUnit.Framework;

	[TestFixture]
	public class MonitorTestCase : BaseContextSetUp
	{
		[Test]
		public void monitor_in_for_req_rep()
		{
			using (var repSocket = base.Context.CreateSocket(SocketType.Rep))
			using (var reqSocket = base.Context.CreateSocket(SocketType.Req))
			// using (var monitor = new Monitor(repSocket, base.Context, "monitor1"))
			{
				var monitor = new Monitor(repSocket, base.Context, "monitor1");
				monitor.SocketEvent += (sender,args) =>
				{
					Console.WriteLine(args.Event + " " + args.Endpoint + " " + args.Error);
				};

				repSocket.Bind("tcp://0.0.0.0:90001");

				reqSocket.Connect("tcp://127.0.0.1:90001");
				reqSocket.Send("Hello");

				var s = repSocket.RecvString();
				repSocket.Send("Reply");

				s = reqSocket.RecvString();
			}
		}
	}
}
