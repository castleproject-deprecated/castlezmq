namespace Castle.Facilities.Zmq.Rpc.Internal
{
	using System;
	using System.IO;
	using Castle.Zmq.Rpc.Model;


	/// <summary>
	/// Builds some complex objects, often involving 
	/// serialization logic and optimizations
	/// </summary>
	internal static class Builder
	{
		public static ResponseMessage BuildResponse(Tuple<object, Type> result)
		{
			if (result.Item2 == typeof(void)) return ResponseMessage.Empty;

			if (ReflectionUtils.IsCollectionType(result.Item2))
			{
				var elements = ReflectionUtils.NormalizeToArray(result.Item1, result.Item2);
				var serialized = Serialization.SerializeArray(elements);
				return new ResponseMessage(serialized, TypeShortnameLookup.GetName(result.Item2), null);
			}
			if (result.Item1 != null)
			{
				var tuple = Serialization.BuildParamTuple(result.Item2, result.Item1);
				return new ResponseMessage(tuple.SerializedValue, tuple.TypeName, null);
			}

			return ResponseMessage.Empty;
		}

		public static ResponseMessage BuildResponse(Exception exception)
		{
			return new ResponseMessage()
			{
				ExceptionInfo = new ExceptionInfo(exception.GetType().Name, exception.Message, exception.StackTrace)
			};
		}

		public static byte[] SerializeRequest(RequestMessage requestMessage)
		{
			return Serialization.Serialize(requestMessage);
		}

		public static RequestMessage DeserializeRequest(byte[] message)
		{
			return Serialization.Deserialize<RequestMessage>(message);
;		}

		public static byte[] SerializeResponse(ResponseMessage response)
		{
			return Serialization.Serialize(response);
		}

		public static ResponseMessage DeserializeResponse(byte[] message)
		{
			return Serialization.Deserialize<ResponseMessage>(message);
			;
		}

		public static ParamTuple[] ParametersToParamTuple(object[] arguments, Type[] parametersTypes)
		{
			if (arguments == null) return null;
			if (parametersTypes.Length == 0) return null;

			// Not using linq for perf reasons

			var retArray = new ParamTuple[parametersTypes.Length];

			for (int i = 0; i < retArray.Length; i++)
			{
				// var pTuple = Serialization.BuildParamTuple(parametersTypes[i], arguments[i]);
				var pTuple = Serialization.BuildParamTuple(arguments[i].GetType(), arguments[i]);
				retArray[i] = pTuple;
			}

			return retArray;
		}

		public static object[] ParamTupleToObjects(ParamTuple[] parameters, Type[] parametersTypes)
		{
			if (parametersTypes.Length == 0) return null;

			// Not using linq for perf reasons

			var args = new object[parameters.Length];

			for (int i = 0; i < args.Length; i++)
			{
				args[i] = Serialization.DeserializeParamTuple(parameters[i], parametersTypes[i]);
			}

			return args;
		}
	}
}