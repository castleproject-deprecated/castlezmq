namespace Castle.Zmq.Rpc.Model
{
	using ProtoBuf;

	[ProtoContract(SkipConstructor = true)]
	public class ParamTuple
	{
		public ParamTuple()
		{
		}

		public ParamTuple(byte[] serializedValue, string typeName)
		{
			SerializedValue = serializedValue;
			TypeName = typeName;
		}

		[ProtoMember(1)]
		public byte[] SerializedValue { get; set; }
		[ProtoMember(2)]
		public string TypeName { get; set; }
	}

	[ProtoContract(SkipConstructor = true)]
	public class RequestMessage
	{
		[ProtoMember(1)]
		public string TargetService { get; set; }
		[ProtoMember(2)]
		public string TargetMethod { get; set; }
		[ProtoMember(3)]
		public ParamTuple[] Params { get; set; }
		[ProtoMember(4)]
		public string[] ParamTypes { get; set; }

	}

	[ProtoContract(SkipConstructor = true)]
	public class ExceptionInfo
	{
		public ExceptionInfo()
		{
		}

		public ExceptionInfo(string typename, string message)
		{
			Typename = typename;
			Message = message;
		}

		[ProtoMember(1)]
		public string Typename { get; set; }
		[ProtoMember(2)]
		public string Message { get; set; }
	}

	[ProtoContract(SkipConstructor = true)]
	public class ResponseMessage
	{
		public ResponseMessage()
		{
		}

		public ResponseMessage(byte[] returnValue, string returnValueType, ExceptionInfo exceptionInfo)
		{
			ReturnValue = returnValue;
			ReturnValueType = returnValueType;
			ExceptionInfo = exceptionInfo;
		}

		[ProtoMember(1)]
		public ExceptionInfo ExceptionInfo { get; set; }
		[ProtoMember(2)]
		public byte[] ReturnValue { get; set; }
		[ProtoMember(3)]
		public string ReturnValueType { get; set; }
	}


}
