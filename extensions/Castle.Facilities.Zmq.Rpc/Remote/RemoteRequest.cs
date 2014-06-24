namespace Castle.Facilities.Zmq.Rpc.Remote
{
	using System;
	using Castle.Zmq;
	using Castle.Zmq.Counters;
	using Castle.Zmq.Extensions;
	using Castle.Zmq.Rpc.Model;


	internal class RemoteRequest : BaseRequest<ResponseMessage>
	{
		private readonly string _endpoint;
		private readonly RequestMessage _requestMessage;
		private readonly SerializationStrategy _serializationStrategy;

		public RemoteRequest(IZmqContext context, string endpoint, 
							 RequestMessage requestMessage, 
							 SerializationStrategy serializationStrategy) : base(context, endpoint)
		{
			_endpoint = endpoint;
			_requestMessage = requestMessage;
			_serializationStrategy = serializationStrategy;
		}

		protected override void SendRequest(IZmqSocket socket)
		{
			var buffer = _serializationStrategy.SerializeRequest(_requestMessage);
			socket.Send(buffer);
			// PerfCounters.IncrementSent ()
		}

		protected override ResponseMessage GetReply(IZmqSocket socket)
		{
			var buffer = socket.Recv();

			if (buffer == null)
			{
				var message = "Remote call took too long to respond. Is the server up? Endpoint: " + 
						_endpoint + " - Current timeout " + this.Timeout;
				return new ResponseMessage(null, null, new ExceptionInfo("Timeout", message));
			}

			return _serializationStrategy.DeserializeResponse(buffer);
		}
	}
}
