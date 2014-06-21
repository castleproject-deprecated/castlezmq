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

		private const int HAUSNUM = 156384712;
		public const int ETERM = HAUSNUM + 53;

		public const int ErrorCode = -1;

		public static int LastError()
		{
			var error = Native.zmq_errno();
			return error;
		}

		public static string LastErrorString(int errorcode = 0)
		{
			if (errorcode== 0) errorcode = Native.zmq_errno();
			var errormsgptr = zmq_strerror(errorcode);
			return Marshal.PtrToStringAnsi(errormsgptr);
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
			var msg = LastErrorString(error);
			throw new ZmqException(msg, error);
		}
	}
}
