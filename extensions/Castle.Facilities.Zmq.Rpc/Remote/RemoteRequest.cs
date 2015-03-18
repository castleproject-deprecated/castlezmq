namespace Castle.Facilities.Zmq.Rpc.Remote
{
	using Castle.Zmq;
	using Castle.Zmq.Extensions;
	using Castle.Zmq.Rpc.Model;


	internal class RemoteRequest : BaseRequest<ResponseMessage>
	{
		public const string TimeoutTypename = "Timeout";

		private readonly string _endpoint;
		private readonly RequestMessage _requestMessage;
		private readonly SerializationStrategy _serializationStrategy;

		public RemoteRequest(IZmqContext context, string endpoint, 
							 RequestMessage requestMessage, 
							 SerializationStrategy serializationStrategy) : base(context, endpoint)
		{
			this._endpoint = endpoint;
			this._requestMessage = requestMessage;
			this._serializationStrategy = serializationStrategy;

			this.Timeout = 30 * 1000;
		}

		protected override void SendRequest(IZmqSocket socket)
		{
			var buffer = _serializationStrategy.SerializeRequest(_requestMessage);
			socket.Send(buffer);
			// PerfCounters.IncrementSent()
		}

		protected override ResponseMessage GetReply(byte[] buffer, IZmqSocket socket, bool hasTimeoutWaitingRecv)
		{
			// var buffer = socket.Recv();

			if (hasTimeoutWaitingRecv)
			{
				var message = "Remote call took too long to respond. Is the server up? Endpoint: " + 
						_endpoint + " - Current timeout " + this.Timeout;
				return new ResponseMessage(null, null, new ExceptionInfo(TimeoutTypename, message));
			}

			return _serializationStrategy.DeserializeResponse(buffer);
		}
	}
}
