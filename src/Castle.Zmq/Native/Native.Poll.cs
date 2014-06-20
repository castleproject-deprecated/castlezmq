namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;

	internal static partial class Native
	{
		internal static class Poll
		{
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
			public struct zmq_pollitem_t
			{
				public IntPtr socket;
#if x64 
				public long fd; 
#else
				public int fd; 
#endif
 				public Int16 events;
				public Int16 revents;
			}

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_poll([In, Out] zmq_pollitem_t[] items, int count, long timeout);
		}
	}
}
