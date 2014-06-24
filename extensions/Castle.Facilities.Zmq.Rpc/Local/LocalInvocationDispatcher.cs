namespace Castle.Facilities.Zmq.Rpc.Local
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Castle.MicroKernel;
	using Castle.Zmq.Rpc.Model;

	/// <summary>
	/// Resolves the service instance and makes the invocation through reflection
	/// </summary>
	public class LocalInvocationDispatcher
	{
		private readonly Func<string, object> _resolver;
		private readonly SerializationStrategy _serialization;

		private Dictionary<string, Type> _typeResCache = new Dictionary<string, Type>(); 


		public LocalInvocationDispatcher(IKernel kernel, SerializationStrategy serialization)
		{
			this._resolver = t =>
			{
				Type type = null;

				lock (_typeResCache)
				{
					if (!_typeResCache.TryGetValue(t, out type))
					{
						type = Type.GetType(t, true);
						_typeResCache[t] = type;
					}
				}

				return kernel.Resolve(type);
			};
			this._serialization = serialization;
		}

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

			var args = this._serialization.DeserializeParams(parameters, paramTypes) ?? new object[0];

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