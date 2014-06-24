namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using System.Collections.Concurrent;
	using System.Linq;
	using Castle.DynamicProxy;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using Castle.Facilities.Zmq.Rpc.Remote;
	using Castle.Zmq;
	using Castle.Zmq.Rpc.Model;

	public class RemoteRequestInterceptor : IInterceptor
	{
		internal static ConcurrentDictionary<string, Type> Typename2Type =
			new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

		private readonly IZmqContext _context;
		private readonly RemoteEndpointRegistry _endpointRegistry;

		public RemoteRequestInterceptor(IZmqContext context, RemoteEndpointRegistry endpointRegistry)
		{
			this._context = context;
			_endpointRegistry = endpointRegistry;
		}

		public void Intercept(IInvocation invocation)
		{
			if (invocation.TargetType != null)
			{
				invocation.Proceed();
			}
			else
			{
				var m = invocation.Method;

				var asm = invocation.Method.DeclaringType.Assembly;
				var service = m.DeclaringType.DeclaringType.AssemblyQualifiedName;
				var endpoint = _endpointRegistry.GetEndpoint(asm);

				var parameters = m.GetParameters();
				var parametersTypes = parameters.Select(p => p.ParameterType).ToArray();

				var serializedArs = Builder.ParametersToParamTuple(invocation.Arguments, parametersTypes);
				var serializedPInfo = Serialization.SerializeParameterTypes(parametersTypes);

				var requestMessage = new RequestMessage(service, m.Name, serializedArs, serializedPInfo);

				var request = new RemoteRequest(_context, endpoint, requestMessage);
				var response = request.Get();

				if (response.ExceptionInfo != null)
				{
					var msg = "Remote server threw " + response.ExceptionInfo.Typename + " with message " +
					          response.ExceptionInfo.Message;
					throw new Exception(msg);
				}

				if (m.ReturnType != typeof (void))
				{
					invocation.ReturnValue = Serialization.DeserializeResponse(response, m.ReturnType);
				}
			}
		}
	}
}