using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Castle.Zmq;

namespace remote_thr
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("usage: remote_thr <connect-to> <message-size> <message-count>");
                Environment.Exit(1);
            }

            var connectTo = args[0];
            var messageSize = int.Parse(args[1]);
            var messageCount = int.Parse(args[2]);

            using (var ctx = new Context())
            using (var s = ctx.CreateSocket(SocketType.Push))
            {
                s.Connect(connectTo);

                var msg = new byte[messageSize];
                for (int i = 0; i < messageCount; i++)
                {
                    s.Send(msg);
                }
            }
        }
    }
}
