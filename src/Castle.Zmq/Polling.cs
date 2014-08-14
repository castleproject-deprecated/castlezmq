namespace Castle.Zmq
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.ExceptionServices;
	using System.Runtime.InteropServices;
	using System.Security;

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
		private readonly Native.Poll.zmq_pollitem_t_x64[] _items_x64;
		private readonly Native.Poll.zmq_pollitem_t_x86[] _items_x86;
		private readonly Dictionary<IntPtr, Socket> _ptr2Socket = new Dictionary<IntPtr, Socket>(); 

		public Polling(PollItem[] items)
		{
			this._items = items;

			if (!Context.IsMono && Environment.Is64BitProcess)
				_items_x64 = new Native.Poll.zmq_pollitem_t_x64[items.Length];
			else
				_items_x86 = new Native.Poll.zmq_pollitem_t_x86[items.Length];

			// caching the ptr (which shouldnt change since they are unmanaged)
			for (int i = 0; i < items.Length; i++)
			{
				var pollItem = items[i];

				if (!Context.IsMono && Environment.Is64BitProcess)
				{
					_items_x64[i] = pollItem.Item64;
					_ptr2Socket[pollItem.Item64.socket] = pollItem._socket;
				}
				else
				{
					_items_x86[i] = pollItem.Item32;
					_ptr2Socket[pollItem.Item32.socket] = pollItem._socket;
				}
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

		[HandleProcessCorruptedStateExceptions, SecurityCritical]
		public bool Poll(int timeout)
		{
			int res = 0;

			try
			{
				if (!Context.IsMono && Environment.Is64BitProcess)
				{
					res = Native.Poll.zmq_poll_x64(_items_x64, _items_x64.Length, timeout);

					if (res == 0) return false; // nothing happened

					if (res > 0)
					{
						this.InternalFireEvents64(_items_x64);
					}
				}
				else
				{
					res = Native.Poll.zmq_poll_x86(_items_x86, _items_x86.Length, timeout);

					if (res == 0) return false; // nothing happened

					if (res > 0)
					{
						this.InternalFireEvents86(_items_x86);
					}
				}

				if (res == Native.ErrorCode)
				{
					var error = Native.LastError();
					
					if (error != ZmqErrorCode.EINTR) // Unix system interruption 
					{
						Native.ThrowZmqError();
					}
				}
			}
			catch (SEHException zex) // rare, but may happen if the endpoint disconnects uncleanly
			{
				var msg = "Error polling socket(s): " + Native.LastErrorString() + " Inner: " + zex.InnerException;
				System.Diagnostics.Trace.TraceError(msg);
				System.Diagnostics.Debug.WriteLine(msg);
				if (LogAdapter.LogEnabled)
				{
					LogAdapter.LogError(this.GetType().FullName, msg);
				}
			}
			return (res > 0);
		}

		/// <summary>
		/// Returns immediately
		/// </summary>
		public bool PollNow()
		{
			return Poll(0);
		}

		/// <summary>
		/// Wont return until gets a signal
		/// </summary>
		public bool PollForever()
		{
			return Poll(-1);
		}

		private void InternalFireEvents64(Native.Poll.zmq_pollitem_t_x64[] items)
		{
			bool hasRcv;
			bool hasSend;

			// for (int i = 0; i < items.Length; i++)
			foreach (var pollitem in items)
			{
				if (pollitem.revents != 0)
				{
					hasRcv = ((pollitem.revents & Native.Poll.POLLIN) != 0);
					hasSend = ((pollitem.revents & Native.Poll.POLLOUT) != 0);

					if (hasRcv || hasSend)
					{
						// get our socket given the pointer
						Socket socket;
						if (_ptr2Socket.TryGetValue(pollitem.socket, out socket))
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

		private void InternalFireEvents86(Native.Poll.zmq_pollitem_t_x86[] items)
		{
			bool hasRcv;
			bool hasSend;

			// for (int i = 0; i < items.Length; i++)
			foreach (var pollitem in items)
			{
				if (pollitem.revents != 0)
				{
					hasRcv = ((pollitem.revents & Native.Poll.POLLIN) != 0);
					hasSend = ((pollitem.revents & Native.Poll.POLLOUT) != 0);

					if (hasRcv || hasSend)
					{
						// get our socket given the pointer
						Socket socket;
						if (_ptr2Socket.TryGetValue(pollitem.socket, out socket))
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
