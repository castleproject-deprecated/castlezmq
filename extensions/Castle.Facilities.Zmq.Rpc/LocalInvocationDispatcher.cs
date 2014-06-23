namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using System.Reflection;
	using Castle.Zmq.Rpc.Model;

	public class LocalInvocationDispatcher
	{
		private readonly Func<string, object> _resolver;

		public LocalInvocationDispatcher(Func<string, object> resolver)
		{
			this._resolver = resolver;
		}

		public Tuple<object, Type> Dispatch(string service, string methodName, ParamTuple[] parameters, string[] pInfo)
		{
			var instance = _resolver(service);

			var method = ResolveMethod(instance.GetType(), methodName, pInfo);

			var args = Serialization.DeserializeParameters(parameters);

			var result = method.Invoke(instance, args);

			return Tuple.Create(result, method.ReturnType);
		}

		private MethodInfo ResolveMethod(Type type, string method, string[] paramTypes)
		{
			throw new NotImplementedException();
		}
	}
}