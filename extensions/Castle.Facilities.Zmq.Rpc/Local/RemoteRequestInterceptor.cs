namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using System.Collections.Concurrent;
	using System.Linq;
	using Castle.DynamicProxy;


	public class RemoteRequestInterceptor : IInterceptor
	{
		internal static ConcurrentDictionary<string, Type> Typename2Type =
			new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);

		private readonly RemoteRequestService _remoteRequestService;

		public RemoteRequestInterceptor(RemoteRequestService remoteRequestService)
		{
			this._remoteRequestService = remoteRequestService;
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
				var asmName = asm.GetName().Name;

				var service = m.DeclaringType.AssemblyQualifiedName;
				var parameters = m.GetParameters();
				var parametersTypes = parameters.Select(p => p.ParameterType).ToArray();

				object retValue = 
					this._remoteRequestService.Invoke(asmName, 
							service, m.Name, invocation.Arguments, parametersTypes, m.ReturnType);

				invocation.ReturnValue = retValue;
			}
		}
	}
}