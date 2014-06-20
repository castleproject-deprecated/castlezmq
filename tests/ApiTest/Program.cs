using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiTest
{
	using System.Threading;
	using Castle.Zmq;

	class Program
	{
		static void Main(string[] args)
		{
			using (var ctx = new Context())
			{
				const string MsgReq = "function shall retrieve the value for the option specified by the option_name argument for the ØMQ socket pointed to by the socket argument, and store it in the buffer pointed to by the option_value argument";
				const string MsgReply = "The option_len argument is the size in bytes of the buffer pointed to by option_value; upon successful completion zmq_getsockopt() shall modify the option_len argument to indicate the actual size of the option value stored in the buffer";

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
	}
}
