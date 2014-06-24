namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using Castle.Zmq.Rpc.Model;

	public abstract class SerializationStrategy
	{
		public abstract ParamTuple[] SerializeParameters(object[] args, Type[] paramTypes);
		public abstract object[] DeserializeParams(ParamTuple[] parameters, Type[] paramTypes);

		public abstract string[] SerializeParameterTypes(Type[] paramTypes);
		public abstract Type[] DeserializeParameterTypes(string[] typeNames);

		public abstract byte[] SerializeRequest(RequestMessage requestMessage);
		public abstract RequestMessage DeserializeRequest(byte[] buffer);

		public abstract byte[] SerializeResponse(ResponseMessage response);
		public abstract ResponseMessage DeserializeResponse(byte[] buffer);

		public abstract object DeserializeResponseValue(ResponseMessage response, Type retType);
	}
}