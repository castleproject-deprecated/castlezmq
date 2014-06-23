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
				if (this.Timeout != Socket.InfiniteTimeout)
				{
					socket.SetOption(SocketOpt.RCVTIMEO, this.Timeout);
				}

				socket.Connect(_endpoint);

				SendRequest(socket);

				return GetReply(socket);
			}
		}

		protected abstract T GetReply(IZmqSocket socket);

		protected abstract void SendRequest(IZmqSocket socket);
	}
}