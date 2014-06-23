namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using System.Collections.Generic;
	using Castle.Zmq.Rpc.Model;



	internal static class Serialization
	{
		public static ParamTuple[] SerializeParameters(object[] arguments, Type[] parametersTypes)
		{
			
		}

		public static string[] SerializeParameterInfo(Type[] parametersTypes)
		{
			// ps |> Array.map (fun p -> p.ParameterType) |> Array.map (fun t -> t.AssemblyQualifiedName)
		}

		public static object DeserializeResponse(ResponseMessage response, Type returnType)
		{
			return null;
		}

		public static RequestMessage DeserializeRequest(byte[] message)
		{
			
		}

		public static ResponseMessage BuildResponse(Tuple<object, Type> result)
		{
			
		}

		public static ResponseMessage BuildResponse(Exception exception)
		{
				
		}

		public static byte[] SerializeResponse(ResponseMessage response)
		{
			
		}
	}
}
