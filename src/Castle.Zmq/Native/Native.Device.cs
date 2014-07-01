namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;

	internal static partial class Native
	{
		internal static class Device
		{
			[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
			public static extern int zmq_proxy(IntPtr front, IntPtr back, IntPtr capture);
		}
	}
}