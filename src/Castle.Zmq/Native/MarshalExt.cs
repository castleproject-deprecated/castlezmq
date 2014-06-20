namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;

	internal static class MarshalExt
	{
		/// <summary>
		/// Allocates an unmanaged buffer of the given size, 
		/// invokes the delegate, and frees the memory
		/// </summary>
		/// <param name="fn">delegate to invoke</param>
		/// <param name="bufferLen">length of the buffer to allocate</param>
		public static void AllocAndRun(Action<IntPtr> fn, int bufferLen)
		{
			var bufferPtr = Marshal.AllocHGlobal(bufferLen);
			try
			{
				fn(bufferPtr);
			}
			finally
			{
				Marshal.FreeHGlobal(bufferPtr);
			}
		}

		public static void AllocAndRun(Action<IntPtr> fn, long bufferLen)
		{
			AllocAndRun(fn, Convert.ToInt32(bufferLen));
		}
	}
}