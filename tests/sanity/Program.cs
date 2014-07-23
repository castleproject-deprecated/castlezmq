namespace sanity
{
	using System;
	using System.Net.Cache;
	using Castle.Zmq;

	class Program
	{
		static void Main(string[] args)
		{
			var isMono = (Type.GetType("Mono.Runtime", false) != null);
			Console.WriteLine("Is Mono? " + isMono);


			using (var ctx = new Context())
			{
				// Create req/rep vanilla in tcp
				var req = ctx.Req();
				var reply = ctx.Rep();
				req.SetOption(SocketOpt.RCVTIMEO, 3000);

				reply.Bind("tcp://0.0.0.0:9000");
				req.Connect("tcp://127.0.0.1:9000");
				req.Send("A cup of coffee please");

				var reqStr = reply.RecvString();
				Console.WriteLine(reqStr);
				reply.Send("Here's your coffee sir");
				var replyStr = req.RecvString();
				Console.WriteLine(replyStr);
				req.Dispose();
				reply.Dispose();


				// Polling

				reply = ctx.Rep();
				reply.Bind("tcp://0.0.0.0:9001");
				var polling = new Polling(PollingEvents.RecvReady, reply);
				polling.RecvReady += socket =>
				{
					reqStr = reply.RecvString();
					reply.Send("Here's another cup of coffee sir");
				};

				req = ctx.Req();
				req.Connect("tcp://127.0.0.1:9001");
				req.Send("Another cup of coffee please");

				polling.PollForever();

				replyStr = req.RecvString();
				Console.WriteLine(replyStr);
				req.Dispose();
				reply.Dispose();


				// Pub sub


				// Devices


			}


		}

	}
}
