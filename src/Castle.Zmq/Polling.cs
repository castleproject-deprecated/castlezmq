namespace Castle.Zmq
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	[Flags]
	public enum PollingEvents
	{
		RecvReady = Native.Poll.POLLIN,
		SendReady = Native.Poll.POLLOUT
	}

	public delegate void SocketEventDelegate(IZmqSocket socket);

	public class Polling
	{
		private readonly PollItem[] _items;
		private readonly Dictionary<IntPtr, Socket> _ptr2Socket = new Dictionary<IntPtr, Socket>(); 

		public Polling(PollItem[] items)
		{
			this._items = items;

			// caching the ptr (which shouldnt change since they are unmanaged)
			foreach (var pollItem in _items)
			{
				if (Environment.Is64BitProcess)
					_ptr2Socket[pollItem.Item64.socket] = pollItem._socket;
				else
					_ptr2Socket[pollItem.Item32.socket] = pollItem._socket;
			}
		}

		public Polling(PollingEvents events, params Socket[] sockets)
			: this(BuildPollItems(events, sockets))
		{
		}

		public Polling(PollingEvents events, params IZmqSocket[] sockets)
			: this(BuildPollItems(events, sockets.Cast<Socket>().ToArray()))
		{
		}

		/// <summary>
		/// Triggered when a socket has buffer to be cosumed (received)
		/// </summary>
		public SocketEventDelegate RecvReady;

		/// <summary>
		/// Triggered when a socket is ready to send more data
		/// </summary>
		public SocketEventDelegate SendReady;

		public void Poll(int timeout)
		{
			int res = 0;

			if (Environment.Is64BitProcess)
			{
				var items = _items.Select(i => i.Item64);
				var array = items.ToArray();
				res = Native.Poll.zmq_poll_x64(array, array.Length, timeout);

				if (res == 0) return; // nothing happened

				if (res > 0)
				{
					this.InternalFireEvents(array.Cast<Native.Poll.IZmq_pollitem>());
				}
			}
			else
			{
				var items = _items.Select(i => i.Item32);
				var array = items.ToArray();
				res = Native.Poll.zmq_poll_x86(array, array.Length, timeout);

				if (res == 0) return; // nothing happened

				if (res > 0)
				{
					this.InternalFireEvents(array.Cast<Native.Poll.IZmq_pollitem>());
				}
			}
	
			if (res == Native.ErrorCode) Native.ThrowZmqError();
		}

		/// <summary>
		/// Returns immediately
		/// </summary>
		public void PollNow()
		{
			Poll(0);
		}

		/// <summary>
		/// Wont return
		/// </summary>
		public void PollForever()
		{
			Poll(-1);
		}

		private void InternalFireEvents(IEnumerable<Native.Poll.IZmq_pollitem> items)
		{
			bool hasRcv;
			bool hasSend;

			// for (int i = 0; i < items.Length; i++)
			foreach (var pollitem in items)
			{
				if (pollitem.Revents != 0)
				{
					hasRcv = ((pollitem.Revents & Native.Poll.POLLIN) != 0);
					hasSend = ((pollitem.Revents & Native.Poll.POLLOUT) != 0);

					if (hasRcv || hasSend)
					{
						// get our socket given the pointer
						Socket socket;
						if (_ptr2Socket.TryGetValue(pollitem.Socket, out socket))
						{
							// fire the events
							if (hasRcv)
							{
								var ev = this.RecvReady;
								if (ev != null) ev(socket);
							}
							if (hasSend)
							{
								var ev = this.SendReady;
								if (ev != null) ev(socket);
							}
						}
					}
				}
			}
		}

		private static PollItem[] BuildPollItems(PollingEvents events, Socket[] sockets)
		{
			var items = new PollItem[sockets.Length];

			for (int i = 0; i < items.Length; i++)
			{
				items[i] = new PollItem(events, sockets[i]);
			}

			return items;
		}
	}
}
