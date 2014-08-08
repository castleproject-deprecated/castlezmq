namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;

	internal static partial class Native
	{
		internal static class Monitor
		{
			[StructLayout(LayoutKind.Sequential)]
			public struct zmq_event_t
			{
				public UInt16 event_;
				public int value;
			}

//			let internal EVENT_DETAIL_SIZE = sizeof<uint16> + sizeof<int32>
//
			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_socket_monitor(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string inproc_endpoint, int events);
		}
	}
}