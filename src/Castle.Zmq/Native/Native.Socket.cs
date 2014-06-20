namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;

	

	internal static partial class Native
	{
		internal static class Socket
		{
			#region Sending flags and ret codes

			/// <summary> Block thread until message frame is sent </summary>
			public const int WAIT = 0;
			/// <summary> Queue message frame for sending (return immediately) </summary>
			public const int DONTWAIT = 1;
			/// <summary> More message frames will follow the current frame </summary>
			public const int SNDMORE = 2;

			// send operation must be tried again (may happen when sending with DONTWAIT)
			public const int EAGAIN = 11;

			#endregion

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr zmq_socket(IntPtr context, int socketType);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_close(IntPtr socketPtr);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_setsockopt(IntPtr socket, int option, IntPtr value, int size);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_getsockopt(IntPtr socket, int option, IntPtr value, IntPtr size);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_bind(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string address);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_connect(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string address);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_unbind(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string address);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_disconnect(IntPtr socket, [MarshalAs(UnmanagedType.LPStr)] string address);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_send(IntPtr socket, byte[] buffer, int length, int flags);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_recv(IntPtr socket, [Out] byte[] buffer, IntPtr length, int flags);

		}
	}
}
