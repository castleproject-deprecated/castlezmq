namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;

	/// <summary>
	/// For internal use (and just as a way to conveniently free the resources)
	/// </summary>
	internal class MsgFrame : IDisposable
	{
		private const int zmq_msg_t_size = 32;
		internal IntPtr _msgPtr;

		public MsgFrame()
		{
			_msgPtr = Marshal.AllocHGlobal(zmq_msg_t_size);
			var res = Native.MsgFrame.zmq_msg_init(_msgPtr);
			if (res == Native.ErrorCode)
			{
				this.Dispose();
				Native.ThrowZmqError();
			}
		}

		public byte[] ToBytes()
		{
			// what's the size of the buffer in zmq_msg_t ?
			var size = Native.MsgFrame.zmq_msg_size(_msgPtr);
			if (size == 0) return new byte[0];

			// gets a pointer to the buffer in zmq_msg_t
			var arrayPtr = Native.MsgFrame.zmq_msg_data(_msgPtr);
			var buffer = new byte[size];

			// copy the bytes
			Marshal.Copy(arrayPtr, buffer, 0, size);
			
			return buffer;
		}

		public void Dispose()
		{
			Native.MsgFrame.zmq_msg_close(_msgPtr); // let's ignore error code in this case

			Marshal.FreeHGlobal(_msgPtr);
			_msgPtr = IntPtr.Zero;
		}
	}
}