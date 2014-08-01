namespace Castle.Zmq.Extensions
{
	using System;

	public abstract class BaseRequest<T>
	{
		private readonly IZmqContext _context;
		private readonly string _endpoint;

		protected BaseRequest(IZmqContext context, string endpoint)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (endpoint == null) throw new ArgumentNullException("endpoint");

			this._context = context;
			this._endpoint = endpoint;

			this.Timeout = Socket.InfiniteTimeout;
		}

		public int Timeout { get; set; }

		public T Get()
		{
			using (var socket = _context.Req())
			{
//				if (this.Timeout != Socket.InfiniteTimeout)
//				{
//					socket.SetOption(SocketOpt.RCVTIMEO, this.Timeout);
//				}

				socket.Connect(_endpoint);

				SendRequest(socket);

				var polling = new Polling(PollingEvents.RecvReady, socket);
				if (polling.Poll(this.Timeout))
				{
					var data = socket.Recv();
					return GetReply(data, socket, false);
				}
				else
				{
					// timeout
					return GetReply(null, socket, true);
				}
				
			}
		}

		protected abstract T GetReply(byte[] data, IZmqSocket socket, bool hasTimeoutWaitingRecv);

		protected abstract void SendRequest(IZmqSocket socket);
	}
}