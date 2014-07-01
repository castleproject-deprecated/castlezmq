using System;
using System.Diagnostics;
using Castle.Zmq;

namespace local_thr
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("usage: local_thr <bind-to> <message-size> <message-count>");
                Environment.Exit(1);
            }

            var bindTo = args[0];
            var messageSize = int.Parse(args[1]);
            var messageCount = int.Parse(args[2]);

            TimeSpan elapsed;
            using (var ctx = new Context())
            using (var s = ctx.CreateSocket(SocketType.Pull))
            {
                s.Bind(bindTo);

                var msg = s.Recv();

                if (msg.Length != messageSize)
                {
                    Console.WriteLine("message of incorrect size received");
                    Environment.Exit(-1);
                }

                var sw = new Stopwatch();

                sw.Start();
                for (int i = 0; i < messageCount - 1; i++)
                {
                    msg = s.Recv();
                    if (msg.Length != messageSize)
                    {
                        Console.WriteLine("message of incorrect size received");
                        Environment.Exit(-1);
                    }
                }
                sw.Stop();

                elapsed = sw.Elapsed;
            }

            double throughput = messageCount / elapsed.TotalSeconds;
            double megabits = (throughput * messageSize * 8) / (1000000);

            Console.WriteLine("message size: {0} B", messageSize);
            Console.WriteLine("message count: {0}", messageCount);
            Console.WriteLine("mean throughput: {0:0} [msg/s]", throughput);
            Console.WriteLine("mean throughput: {0:0.000} [Mb/s]", megabits);
        }
    }
}
