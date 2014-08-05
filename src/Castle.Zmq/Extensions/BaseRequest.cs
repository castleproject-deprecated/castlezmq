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

		public RequestPoll ReqPoll { get; set; }

		public int Timeout { get; set; }

		public T Get()
		{
			if (ReqPoll != null)
			{
				var socket = ReqPoll.Get(_endpoint);
				var inError = false;

				try
				{
					var tuple = SendReqAndWaitReply(socket);
					inError = tuple.Item2;
					return tuple.Item1;
				}
				finally
				{
					ReqPoll.Return(socket, _endpoint, inError);
				}
			}

			using (var socket = _context.Req())
			{
//				if (this.Timeout != Socket.InfiniteTimeout)
//				{
//					socket.SetOption(SocketOpt.RCVTIMEO, this.Timeout);
//				}

				socket.Connect(_endpoint);

				var tuple = SendReqAndWaitReply(socket);
				return tuple.Item1;
			}
		}

		private Tuple<T, bool> SendReqAndWaitReply(IZmqSocket socket)
		{
			SendRequest(socket);

			var polling = new Polling(PollingEvents.RecvReady, socket);
			if (polling.Poll(this.Timeout))
			{
				var data = socket.Recv();
				var ret = GetReply(data, socket, false);
				return Tuple.Create(ret, false);
			}
			else
			{
				// timeout
				var ret = GetReply(null, socket, true);
				return Tuple.Create(ret, true);
			}
		}

		protected abstract T GetReply(byte[] data, IZmqSocket socket, bool hasTimeoutWaitingRecv);

		protected abstract void SendRequest(IZmqSocket socket);
	}
}