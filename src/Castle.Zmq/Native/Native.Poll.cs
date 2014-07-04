namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;

	internal static partial class Native
	{
		

		internal static class Poll
		{
//			internal interface IZmq_pollitem
//			{
//				IntPtr Socket { get; }
//				Int16 Events { get; }
//				Int16 Revents { get; }
//			}

			// poll for inbound messages
			public const Int16 POLLIN = 1;

			// poll for outbound messages
			public const Int16 POLLOUT = 2;

			//			public const Int16 POLLERR = 4;

			//  indicates polling should exit immediately
			public const long NOW = 0L;
			//  indicates polling should wait indefinitely 
			public const long FOREVER = -1L;

			[StructLayout(LayoutKind.Sequential)]
			public struct zmq_pollitem_t_x86 // : IZmq_pollitem
			{
				public IntPtr socket;
				public int fd; 
				public Int16 events;
				public Int16 revents;

//				public IntPtr Socket {get { return socket; }}
//				public Int16 Events { get { return events; } }
//				public Int16 Revents { get { return revents; } }
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct zmq_pollitem_t_x64 // : IZmq_pollitem
			{
				public IntPtr socket;
				public long fd; 
 				public Int16 events;
				public Int16 revents;

//				public IntPtr Socket { get { return socket; } }
//				public Int16 Events { get { return events; } }
//				public Int16 Revents { get { return revents; } }
			}

			[DllImport("libzmq", EntryPoint = "zmq_poll", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_poll_x86([In, Out] zmq_pollitem_t_x86[] items, int count, long timeout);


			[DllImport("libzmq", EntryPoint = "zmq_poll", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_poll_x64([In, Out] zmq_pollitem_t_x64[] items, int count, long timeout);
		}
	}
}
