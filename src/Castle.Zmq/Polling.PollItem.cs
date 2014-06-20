namespace Castle.Zmq
{
	using System;

	public class PollItem
	{
		internal readonly Socket _socket;
		internal Native.Poll.zmq_pollitem_t Item;

		public PollItem(PollingEvents events, Socket socket)
		{
			_socket = socket;
			var flags = (Int16)events;

			this.Item = new Native.Poll.zmq_pollitem_t()
			{
				socket = socket.SocketPtr,
				events = flags
			};
		}
	}
}
