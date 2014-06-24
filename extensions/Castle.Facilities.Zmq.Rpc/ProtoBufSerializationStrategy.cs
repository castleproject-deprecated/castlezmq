namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using Castle.Zmq.Rpc.Model;

	public class ProtoBufSerializationStrategy : SerializationStrategy
	{
		public override object[] DeserializeParams(ParamTuple[] parameters, Type[] paramTypes)
		{
			return Builder.ParamTupleToObjects(parameters, paramTypes);
		}

		public override Type[] DeserializeParameterTypes(string[] pInfo)
		{
			return Serialization.DeserializeParameterTypes(pInfo);
		}

		public override ParamTuple[] SerializeParameters(object[] args, Type[] paramTypes)
		{
			return Builder.ParametersToParamTuple(args, paramTypes);
		}

		public override string[] SerializeParameterTypes(Type[] paramTypes)
		{
			return Serialization.SerializeParameterTypes(paramTypes);
		}

		public override byte[] SerializeRequest(RequestMessage requestMessage)
		{
			return Builder.SerializeRequest(requestMessage);
		}

		public override RequestMessage DeserializeRequest(byte[] buffer)
		{
			return Builder.DeserializeRequest(buffer);
		}

		public override byte[] SerializeResponse(ResponseMessage response)
		{
			return Builder.SerializeResponse(response);
		}

		public override ResponseMessage DeserializeResponse(byte[] buffer)
		{
			return Builder.DeserializeResponse(buffer);
		}
	}
}