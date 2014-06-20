namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;


	internal static partial class Native
	{
		#region Other stuff

		[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
		public static extern int zmq_errno();

		[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zmq_strerror(int errno);

//		[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//		extern void zmq_version([Out] int& major,[Out] int& minor,[Out] int& patch)

		#endregion

		public const int ErrorCode = -1;

		public static int LastError()
		{
			var error = Native.zmq_errno();
			return error;
		}

		/// <summary>
		/// Should be called when a native zmq function returns error (-1)
		/// </summary>
		public static void ThrowZmqError()
		{
			var error = Native.zmq_errno();
			ThrowZmqError(error);
		}

		public static void ThrowZmqError(int error)
		{
			var errormsgptr = zmq_strerror(error);
			if (errormsgptr != IntPtr.Zero)
			{
				var msg = Marshal.PtrToStringAnsi(errormsgptr);
				throw new ZmqException(msg, error);
			}
		}
	}
}
