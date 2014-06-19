namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;


	internal static partial class Native
	{
		internal static class Context
		{
			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr zmq_ctx_new();

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_ctx_term(IntPtr contextPtr);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_ctx_shutdown(IntPtr contextPtr);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_ctx_set(IntPtr contextPtr, int option, int value);

			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_ctx_get(IntPtr contextPtr, int option);

			public const int IO_THREADS = 1;
			public const int MAX_SOCKETS = 1;
		}
	}
}
