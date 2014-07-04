namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;


	internal static partial class Native
	{
		#region Other stuff

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        
		[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
		public static extern int zmq_errno();

		[DllImport("libzmq", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr zmq_strerror(int errno);

//		[DllImport("libzmq",CallingConvention = CallingConvention.Cdecl)]
//		extern void zmq_version([Out] int& major,[Out] int& minor,[Out] int& patch)

		#endregion

		private const int HAUSNUM = 156384712;
		public const int ETERM = HAUSNUM + 53;
//		public const int ETERM = HAUSNUM + 53;

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
		public static void ThrowZmqError(string context = null)
		{
			var error = Native.zmq_errno();
			ThrowZmqError(error, context);
		}

		public static void ThrowZmqError(int error, string context = null)
		{
			var msg = LastErrorString(error);
			if (context == null)
				throw new ZmqException(msg, error);
			throw new ZmqException(msg + " in " + context, error);
		}
	}
}
