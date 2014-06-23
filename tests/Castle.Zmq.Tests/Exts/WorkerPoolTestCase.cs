namespace Castle.Zmq.Tests.Exts
{
	using System;
	using System.Threading;
	using System.Threading.Tasks;
	using Castle.Zmq.Extensions;
	using NUnit.Framework;

	[TestFixture]
	public class WorkerPoolTestCase : BaseContextSetUp
 	{
		[Test]
		public void workerpool_distributes_items_to_workers()
		{
			var counter = 0;

			Action<byte[], IZmqSocket> proc = (data, socket) =>
			{
				Interlocked.Increment(ref counter);

				// var data = socket.Recv();
				Thread.Sleep(1); // preparing beverage
				socket.Send("done");
			};

			// Creates a wpool with 10 workers
			using (var pool = new WorkerPool(base.Context, "tcp://0.0.0.0:8000", "inproc://workerp1", proc, 10))
			{
				pool.Start();

				// Sends a 100 request for coffee
				// just like Monday 9am, at any starbucks
				for (int i = 0; i < 100; i++)
					using (var req = base.Context.Req())
					{
						req.Connect("tcp://127.0.0.1:8000");
						req.Send("Venti Mocha please!");
						var res = req.RecvString();
						Assert.AreEqual("done", res);
					}

				Assert.AreEqual(100, counter);
			}
		}
	}
}
