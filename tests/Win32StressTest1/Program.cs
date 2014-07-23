namespace Win32StressTest1
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Castle.Zmq;
	using Castle.Zmq.Extensions;

	class Program
	{
		static void Main(string[] args)
		{
			var ctx = new Context();

			Console.CancelKeyPress += (sender, eventArgs) =>
			{
				Console.WriteLine("Stopping...");

				ctx.Dispose();
			};

//			ReqRepTest(ctx);
//
//			PubSubWithPolling(ctx);

			PubSubWithExtensions(ctx);
		}

		private static void PubSubWithExtensions(Context ctx)
		{
			var pub = new MyPub(ctx, "tcp://0.0.0.0:8809");

			var sub1 = new MySub(ctx, "tcp://127.0.0.1:8809");
			var sub2 = new MySub(ctx, "tcp://127.0.0.1:8809");
			var sub3 = new MySub(ctx, "tcp://127.0.0.1:8809");

			sub1.Start();
			sub2.Start();
			sub3.Start();

			pub.Start();

			Task.Factory.StartNew(() =>
			{
				while (true)
				{
					pub.Publish("t1", new MyMessage() { Msg = "Upon successful completion, the zmq_poll() function shall return the number of zmq_pollitem_t structures with events signaled in revents or 0 if no events have been signaled. Upon failure, zmq_poll() shall return -1 and set errno to one of the values defined below." });

					pub.Publish("t2", new MyMessage() { Msg = "Upon successful completion, the zmq_poll() function shall return the number of zmq_pollitem_t structures with events signaled in revents or 0 if no events have been signaled. Upon failure, zmq_poll() shall return -1 and set errno to one of the values defined below." });

					pub.Publish("t3", new MyMessage() { Msg = "Upon successful completion, the zmq_poll() function shall return the number of zmq_pollitem_t structures with events signaled in revents or 0 if no events have been signaled. Upon failure, zmq_poll() shall return -1 and set errno to one of the values defined below." });

					Thread.Sleep(1);
				}
			});

			Thread.CurrentThread.Join();
		}

		private static void PubSubWithPolling(Context ctx)
		{
			using (var pubSocket = ctx.CreateSocket(SocketType.Pub))
			using (var subSocket1 = ctx.CreateSocket(SocketType.Sub))
			using (var subSocket2 = ctx.CreateSocket(SocketType.Sub))
			using (var subSocket3 = ctx.CreateSocket(SocketType.Sub))
			{
				pubSocket.Bind("tcp://0.0.0.0:8809");

				subSocket1.Connect("tcp://127.0.0.1:8809");
				subSocket2.Connect("tcp://127.0.0.1:8809");
				subSocket3.Connect("tcp://127.0.0.1:8809");

				subSocket1.Subscribe("t1");
				subSocket2.Subscribe("t2");
				subSocket3.Subscribe("t3");

				Task.Factory.StartNew(() =>
				{
					while (true)
					{
						pubSocket.Send("t1", hasMoreToSend: true);
						pubSocket.Send("Upon successful completion, the zmq_poll() function shall return the number of zmq_pollitem_t structures with events signaled in revents or 0 if no events have been signaled. Upon failure, zmq_poll() shall return -1 and set errno to one of the values defined below.");

						pubSocket.Send("t2", hasMoreToSend: true);
						pubSocket.Send("The zmq_poll() function may be implemented or emulated using operating system interfaces other than poll(), and as such may be subject to the limits of those interfaces in ways not defined in this documentation.");

						pubSocket.Send("t3", hasMoreToSend: true);
						pubSocket.Send("For standard sockets, this flag is passed through zmq_poll() to the underlying poll() system call and generally means that some sort of error condition is present on the socket specified by fd. For ØMQ sockets this flag has no effect if set in events, and shall never be returned in revents by zmq_poll().");

						Thread.Sleep(1);
					}
				});

				var p = new Polling(PollingEvents.RecvReady, subSocket1, subSocket2, subSocket3);

				p.RecvReady += socket =>
				{
					var topic = socket.Recv();
					if (topic.Length != 2) throw new Exception("unexpected topic");

					var data = socket.Recv();
					if (data.Length < 30) throw new Exception("unexpected data");

//					Console.Write(".");
				};

				ulong counter = 0;

				Task.Factory.StartNew(() =>
				{
					while (true)
					{
						counter++;

						if (counter % 1000 == 0)
						{
							var collCount0 = GC.CollectionCount(0);
							var collCount1 = GC.CollectionCount(1);
							var collCount2 = GC.CollectionCount(2);

							Console.WriteLine("");
							Console.WriteLine("{3}\tCollections on Gen 0 {0}, Gen 1 {1}, Gen 2 {2} ", collCount0, collCount1, collCount2, counter);
						}

						p.PollForever();
					}
				});

				Thread.CurrentThread.Join();
			}
		}

		private static void ReqRepTest(Context ctx)
		{
			const string MsgReq =
					"function shall retrieve the value for the option specified by the option_name argument for the ØMQ socket pointed to by the socket argument, and store it in the buffer pointed to by the option_value argument";
			const string MsgReply =
				"The option_len argument is the size in bytes of the buffer pointed to by option_value; upon successful completion zmq_getsockopt() shall modify the option_len argument to indicate the actual size of the option value stored in the buffer";

			ulong counter = 0;

			using (var repSocket = ctx.CreateSocket(SocketType.Rep))
			{
				repSocket.SetOption(SocketOpt.LINGER, false);
				repSocket.Bind("tcp://0.0.0.0:8801");

				while (true)
				{
					counter++;

					if (counter % 1000 == 0)
					{
						var collCount0 = GC.CollectionCount(0);
						var collCount1 = GC.CollectionCount(1);
						var collCount2 = GC.CollectionCount(2);

						Console.WriteLine("Collections on Gen 0 {0}, Gen 1 {1}, Gen 2 {2} ", collCount0, collCount1, collCount2);
					}

					using (var reqSocket = ctx.CreateSocket(SocketType.Req))
					{
						reqSocket.SetOption(SocketOpt.LINGER, false);

						reqSocket.Connect("tcp://127.0.0.1:8801");

						reqSocket.Send(MsgReq);

						var msg = repSocket.Recv();
						var msgStr = Encoding.UTF8.GetString(msg);

						if (MsgReq != msgStr) throw new Exception("MsgReq is different from expected: " + msgStr);

						repSocket.Send(MsgReply);

						msg = reqSocket.Recv();
						msgStr = Encoding.UTF8.GetString(msg);

						if (MsgReply != msgStr) throw new Exception("MsgReply is different from expected: " + msgStr);
					}
				}
			}
		}
	}

	internal class MyPub : BasePublisher<MyMessage>
	{
		public MyPub(IZmqContext context, string endpoint) : base(context, endpoint, Serializer)
		{
		}

		private static byte[] Serializer(MyMessage arg)
		{
			return Encoding.UTF8.GetBytes(arg.Msg);
		}
	}

	internal class MySub : BaseSubscriber<MyMessage>
	{
		public MySub(IZmqContext context, string endpoint) : base(context, endpoint, Deserializer)
		{
		}

		public override void Start()
		{
			base.Start();

			SubscribeToTopic("");
		}

		private static MyMessage Deserializer(byte[] arg)
		{
			var m = Encoding.UTF8.GetString(arg);
			return new MyMessage() { Msg = m };
		}

		protected override void OnReceived(string topic, MyMessage message)
		{
			
		}
	}

	internal class MyMessage
	{
		public string Msg { get; set; }
		
	}
}
