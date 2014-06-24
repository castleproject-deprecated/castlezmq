namespace Castle.Facilities.Zmq.Rpc.Local
{
	using System;
	using System.Reflection;
	using Castle.Zmq.Rpc.Model;

	public class LocalInvocationDispatcher
	{
		private readonly Func<string, object> _resolver;
		private readonly SerializationStrategy _serialization;

		public LocalInvocationDispatcher(Func<string, object> resolver, SerializationStrategy serialization)
		{
			this._resolver = resolver;
			this._serialization = serialization;
		}

		public Tuple<object, Type> Dispatch(string service, string methodName, ParamTuple[] parameters, string[] pInfo)
		{
			var instance = _resolver(service);

			var paramTypes = this._serialization.DeserializeParameterTypes(pInfo); 
			var method = ResolveMethod(instance.GetType(), methodName, paramTypes);

			var args = this._serialization.DeserializeParams(parameters, paramTypes);

			var result = method.Invoke(instance, args);

			return Tuple.Create(result, method.ReturnType);
		}

		private MethodInfo ResolveMethod(Type targeType, string methodName, Type[] paramTypes)
		{
			// TODO: cache this
			var method = targeType.GetMethod(methodName, paramTypes);

			if (method == null )
				throw new Exception("Could not find method " + methodName + " for type " + targeType.AssemblyQualifiedName);

			return method;
		}
	}
}