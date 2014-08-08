namespace Castle.Zmq
{
	using System;

	public class PollItem
	{
		internal readonly Socket _socket;
		internal Native.Poll.zmq_pollitem_t_x86 Item32;
		internal Native.Poll.zmq_pollitem_t_x64 Item64;

		public PollItem(PollingEvents events, Socket socket)
		{
			_socket = socket;
			var flags = (Int16)events;

			if (!Context.IsMono && Environment.Is64BitProcess)
			{
				this.Item64 = new Native.Poll.zmq_pollitem_t_x64()
				{
					socket = socket.Handle(),
					events = flags
				};
			}
			else
			{
				this.Item32 = new Native.Poll.zmq_pollitem_t_x86()
				{
					socket = socket.Handle(),
					events = flags
				};
			}
		}
	}
}
