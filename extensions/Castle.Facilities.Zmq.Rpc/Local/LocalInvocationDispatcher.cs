namespace Castle.Facilities.Zmq.Rpc.Local
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Castle.MicroKernel;
	using Castle.Zmq.Rpc.Model;

	/// <summary>
	/// Resolves the service instance and makes the invocation through reflection
	/// </summary>
	public class LocalInvocationDispatcher
	{
		private readonly Func<string, Tuple<object, Type>> _resolver;
		private readonly SerializationStrategy _serialization;

		private readonly Dictionary<string, Type> _typeResCache = new Dictionary<string, Type>(); 


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

				return Tuple.Create(kernel.Resolve(type), type);
			};
			this._serialization = serialization;
		}

		public LocalInvocationDispatcher(Func<string, Tuple<object, Type>> resolver, SerializationStrategy serialization)
		{
			this._resolver = resolver;
			this._serialization = serialization;
		}

		public Tuple<object, Type> Dispatch(string service, string methodName, ParamTuple[] parameters, string[] paramsTypeNames)
		{
			var tuple = _resolver(service);
			var instance = tuple.Item1;
			var targetType = tuple.Item2;

			Type[] paramTypes;
			MethodInfo method;

			if (paramsTypeNames == null) // remote caller didnt supply param types, so we try to resolve it without overload support
			{
				method = ResolveMethodWithoutOverloadSupport(targetType, methodName);
				paramTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
			}
			else // parameter types were supplied, so we try to resolve the exact method
			{
				paramTypes = this._serialization.DeserializeParameterTypes(paramsTypeNames);
				method = ResolveMethod(targetType, methodName, paramTypes);
			}


			var args = this._serialization.DeserializeParams(parameters, paramTypes) ?? new object[0];

			var result = method.Invoke(instance, args);

			return Tuple.Create(result, method.ReturnType);
		}

		private MethodInfo ResolveMethod(Type targeType, string methodName, Type[] paramTypes)
		{
			// TODO: cache this
			var method = targeType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, paramTypes, null);

			if (method == null )
				throw new Exception("Could not find method " + methodName + " for type " + targeType.AssemblyQualifiedName);

			return method;
		}

		private MethodInfo ResolveMethodWithoutOverloadSupport(Type targeType, string methodName)
		{
			// TODO: cache this
			var methods = 
				targeType
					.GetMethods(BindingFlags.Public | BindingFlags.Instance)
					.Where(m => m.Name == methodName)
					.ToArray();

			if (methods.Length > 1)
			{
				throw new Exception("Could not find method exact overload of " + methodName + 
									" for type " + targeType.AssemblyQualifiedName + " as the param types were not supplied");
			}

			if (methods.Length == 0)
			{
				throw new Exception("Could not find method " + methodName + " for type " + targeType.AssemblyQualifiedName);
			}

			return methods[0];
		}
	}
}