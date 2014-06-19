namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;


	internal static partial class Native
	{
		internal static class Socket
		{
			// send operation must be tried again
			public const int EAGAIN = 11;

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr zmq_socket(IntPtr context, int socketType);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_close(IntPtr socketPtr);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_setsockopt(IntPtr socket, int option, IntPtr value, IntPtr size);


			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_getsockopt(IntPtr socket, int option, IntPtr value, [Out] IntPtr size);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_bind(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string address);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_connect(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string address);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_unbind(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string address);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_disconnect(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string address);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_send(IntPtr socket, byte[] buffer, IntPtr length, int flags);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_recv(IntPtr socket, [Out] byte[] buffer, IntPtr length, int flags);

		}
	}
}
