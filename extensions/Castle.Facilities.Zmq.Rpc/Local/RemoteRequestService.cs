namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using Castle.Facilities.Zmq.Rpc.Remote;
	using Castle.Zmq;
	using Castle.Zmq.Extensions;
	using Castle.Zmq.Rpc.Model;

	public class RemoteRequestService
	{
		private readonly IZmqContext _context;
		private readonly RemoteEndpointRegistry _endpointRegistry;
		private readonly SerializationStrategy _serializationStrategy;
		private readonly RequestPoll _requestPoll;

		public RemoteRequestService(IZmqContext context, 
									RemoteEndpointRegistry endpointRegistry, 
									SerializationStrategy serializationStrategy, 
									RequestPoll requestPoll)
		{
			this._context = context;
			this._endpointRegistry = endpointRegistry;
			this._serializationStrategy = serializationStrategy;
			this._requestPoll = requestPoll;
		}

		public object Invoke(string host, string service, string methodName, 
							 object[] parameters, Type[] parametersTypes, Type retType)
		{
			var endpoint = this._endpointRegistry.GetEndpoint(host);

			var serializedArs = this._serializationStrategy.SerializeParameters(parameters, parametersTypes);
			var serializedPInfo = this._serializationStrategy.SerializeParameterTypes(parametersTypes);

			var requestMessage = new RequestMessage(service, methodName, serializedArs, serializedPInfo);

			var request = new RemoteRequest(this._context, endpoint, requestMessage, this._serializationStrategy)
			{
				ReqPoll = this._requestPoll
			};

			var watch = System.Diagnostics.Stopwatch.StartNew();
			if (Castle.Zmq.LogAdapter.LogEnabled)
			{
				Castle.Zmq.LogAdapter.LogDebug(
					"Castle.Facilities.Zmq.Rpc.RemoteRequestService", 
					"About to send request for " + host + "-" + service + "-" + methodName);
			}
			
			ResponseMessage response = request.Get();

			watch.Stop();
			if (Castle.Zmq.LogAdapter.LogEnabled)
			{
				Castle.Zmq.LogAdapter.LogDebug(
					"Castle.Facilities.Zmq.Rpc.RemoteRequestService",
					"Reply from " + host + "-" + service + "-" + methodName + 
					" took " + watch.ElapsedMilliseconds + "ms (" + watch.Elapsed.TotalSeconds + "s). " + 
					"Exception? " + (response.ExceptionInfo != null));
			}

			if (response.ExceptionInfo != null)
			{
				var msg = "Remote server or invoker threw " + response.ExceptionInfo.Typename + " with message " +
				          response.ExceptionInfo.Message;
				throw new Exception(msg);
			}

			if (retType != typeof(void))
			{
				return _serializationStrategy.DeserializeResponseValue(response, retType);
			}

			

			return null;
		}
	}
}