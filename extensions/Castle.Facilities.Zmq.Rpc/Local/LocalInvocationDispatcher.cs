namespace Castle.Facilities.Zmq.Rpc.Local
{
	using System;
	using System.Reflection;
	using Castle.Facilities.Zmq.Rpc.Internal;
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

			var paramTypes = Serialization.DeserializeParameterTypes(pInfo);
			var method = ResolveMethod(instance.GetType(), methodName, paramTypes);

			var args = Builder.ParamTupleToObjects(parameters, paramTypes);

			var result = method.Invoke(instance, args);

			return Tuple.Create(result, method.ReturnType);
		}

		private MethodInfo ResolveMethod(Type targeType, string methodName, Type[] paramTypes)
		{
			throw new NotImplementedException();
		}
	}
}