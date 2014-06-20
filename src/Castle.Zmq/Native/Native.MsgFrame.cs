namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;


	internal static partial class Native
	{
		internal static class MsgFrame
		{
			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_msg_init(IntPtr msgPtr);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_msg_close(IntPtr msgPtr);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_msg_recv(IntPtr msgPtr, IntPtr socket, int flags);

//			[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//			public static extern int zmq_msg_init_size(zmq_msg_t msg, size_t size)
//
//			[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//			public static extern int zmq_msg_move(zmq_msg_t target, zmq_msg_t source)
//
//			[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//			public static extern int zmq_msg_copy(zmq_msg_t target, zmq_msg_t source)
//
//			[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//			public static extern int zmq_msg_send(zmq_msg_t msg, HANDLE socket, int flags)
//
//
//
			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr zmq_msg_data(IntPtr msgPtr);
//
			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_msg_size(IntPtr msgPtr);

//
//			[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//			public static extern int zmq_msg_more(zmq_msg_t msg)
//
//			[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//			public static extern int zmq_msg_get(zmq_msg_t msg, int option)
//
//			[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//			public static extern int zmq_msg_set(zmq_msg_t msg, int option, int optval)
		}
	}
}
