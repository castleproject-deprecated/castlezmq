namespace Castle.Facilities.Zmq.Rpc.Internal
{
	using System;
	using Castle.Zmq.Rpc.Model;

	internal static class Builder
	{
		public static ResponseMessage BuildResponse(Tuple<object, Type> result)
		{
			throw new NotImplementedException();
		}

		public static ResponseMessage BuildResponse(Exception exception)
		{
			throw new NotImplementedException();
		}

		public static byte[] SerializeRequest(RequestMessage requestMessage)
		{
			throw new NotImplementedException();
		}

		public static byte[] SerializeResponse(ResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public static ParamTuple[] ParametersToParamTuple(object[] arguments, Type[] parametersTypes)
		{
			if (parametersTypes.Length == 0) return null;

			// Not using linq for perf reasons

			var retArray = new ParamTuple[parametersTypes.Length];

			for (int i = 0; i < retArray.Length; i++)
			{
				var pTuple = Serialization.BuildParamTuple(parametersTypes[i], arguments[i]);
				retArray[i] = pTuple;
			}

			return retArray;
		}

		public static object[] ParamTupleToObjects(ParamTuple[] parameters, Type[] parametersTypes)
		{
			if (parameters.Length == 0) return null;

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