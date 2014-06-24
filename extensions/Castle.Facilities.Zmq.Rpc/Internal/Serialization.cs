namespace Castle.Facilities.Zmq.Rpc.Internal
{
	using System;
	using System.Collections;
	using System.IO;
	using Castle.Zmq.Rpc.Model;


	internal static class Serialization
	{
		private static readonly byte[] EmptyBuffer = new byte[0];


		public static byte[] Serialize<T>(T instance)
		{
			var stream = new MemoryStream();
			ProtoBuf.Serializer.Serialize(stream, instance);
			return InternalSliceBuffer(stream.GetBuffer(), (int)stream.Length);
		}

		public static T Deserialize<T>(byte[] buffer)
		{
			var stream = new MemoryStream(buffer);
			return ProtoBuf.Serializer.Deserialize<T>(stream);
		}

		public static byte[] SerializeArray<T>(T[] elements)
		{
			var stream = new MemoryStream();
			ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, elements);
			return InternalSliceBuffer(stream.GetBuffer(), (int) stream.Length);
		}

		public static IList DeserializeArray(byte[] buffer, Type collType)
		{
			var stream = new MemoryStream(buffer);
			var res = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, collType);
			return (IList) res;
		}

		public static string[] SerializeParameterTypes(Type[] parametersTypes)
		{
			// not using linq on purpose

			var names = new string[parametersTypes.Length];

			for (int i = 0; i < parametersTypes.Length; i++)
			{
				names[i] = TypeShortnameLookup.GetName(parametersTypes[i]);
			}

			return names;
		}

		public static Type[] DeserializeParameterTypes(string[] typeNames)
		{
			// not using linq on purpose

			var types = new Type[typeNames.Length];

			for (int i = 0; i < typeNames.Length; i++)
			{
				types[i] = TypeShortnameLookup.GetType(typeNames[i]);
			}

			return types;
		}

		public static object DeserializeResponseValue(ResponseMessage response, Type returnType)
		{
			if (response == null) return null;
			if (returnType == typeof (void)) return null;

			if (ReflectionUtils.IsCollectionType(returnType))
			{
				var items = DeserializeArray(response.ReturnValue, returnType);

				if (returnType.IsArray)
				{
					// not strongly typed array
					if (returnType.GetElementType() == typeof (object))
					{
						return items;
					}

					return ReflectionUtils.MakeStronglyTypedArray(returnType.GetElementType(), items);
				}
				
				// Some other type of collection

				var itemType = returnType.GetGenericArguments()[0];

				return ReflectionUtils.MakeStronglyTypedEnumerable(itemType, items);
			}

			var paramTuple = new ParamTuple(response.ReturnValue, response.ReturnValueType);

			return DeserializeParamTuple(paramTuple, returnType);
		}

		public static ParamTuple BuildParamTuple(Type type, object value)
		{
			if (type.IsArray) throw new ArgumentException("Facility doesnt support array as parameters", "type");

			if (value == null) return null;

			// cover primitives and string
			var valueAsStr = value.ToString();

			if (type == typeof (decimal))
			{
				valueAsStr = value.ToString();
			}
			else if (type == typeof (Guid))
			{
				valueAsStr = value.ToString();
			}
			else if (type == typeof (DateTime))
			{
				var dt = ((DateTime) value);
				valueAsStr = dt.Ticks + "|" + (int)dt.Kind;
			}
			else if (type.IsEnum)
			{
				valueAsStr = Convert.ToInt32(value).ToString();
			}
			else if (!type.IsPrimitive && type != typeof(string))
			{
				valueAsStr = null;
			}

			var valueToUse = valueAsStr ?? value;
			var typeBeingSerialized = valueAsStr != null ? "string" : type.AssemblyQualifiedName;

			var buffer = InternalSerialize(valueToUse);
			return new ParamTuple(buffer, typeBeingSerialized);
		}

		public static object DeserializeParamTuple(ParamTuple paramTuple, Type expectedType)
		{
			if (paramTuple == null) return null;

			// should we cache this lookup?
			var envelopedType =
				paramTuple.TypeName == "string" ? typeof (string) : Type.GetType(paramTuple.TypeName, true);

			var res = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(
				new MemoryStream(paramTuple.SerializedValue), null, envelopedType);

			if (expectedType == envelopedType) // best case scenario
			{
				return res;
			}

			if (expectedType == typeof(Int16))
				return Convert.ToInt16((string)res);
			
			if (expectedType == typeof(Int32))
				return Convert.ToInt32((string)res);
			
			if (expectedType == typeof (Int64))
				return Convert.ToInt64((string)res);

			if (expectedType == typeof(UInt16))
				return Convert.ToUInt16((string)res);

			if (expectedType == typeof(UInt32))
				return Convert.ToUInt32((string)res);

			if (expectedType == typeof(UInt64))
				return Convert.ToUInt64((string)res);

			if (expectedType == typeof (decimal))
				return Convert.ToDecimal((string)res);

			if (expectedType == typeof(Single))
				return Convert.ToSingle((string)res);

			if (expectedType == typeof(Double))
				return Convert.ToDouble((string)res);

			if (expectedType == typeof(Guid))
				return Guid.Parse((string)res);

			if (expectedType == typeof (DateTime))
			{
				var parts = res.ToString().Split(new [] {'|'});
				var ticks = Convert.ToInt64(parts[0]);
				var kind = Convert.ToInt32(parts[1]);
				return new DateTime(ticks, (DateTimeKind) kind);
			}

			// not special case, got to be something known to protobuf (has a contract)
			return res;
		}

		private static byte[] InternalSerialize(object value)
		{
			var stream = new MemoryStream();
			ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, value);
			return InternalSliceBuffer(stream.GetBuffer(), (int)stream.Length);
		}

		private static byte[] InternalSliceBuffer(byte[] originalBuffer, int length)
		{
			if (length == originalBuffer.Length) return originalBuffer;
			if (length == 0) return EmptyBuffer;
			
			var smallerBuffer = new byte[length];
			Buffer.BlockCopy(originalBuffer, 0, smallerBuffer, 0, length);
			return smallerBuffer;
		}

		// protobuf-net doesnt deal with arraysegments yet

//		private static ArraySegment<byte> InternalSliceBuffer(byte[] originalBuffer, int length)
//		{
//			if (length == originalBuffer.Length)
//				return new ArraySegment<byte>(originalBuffer);
//			return new ArraySegment<byte>(originalBuffer, 0, length);
//		}
		
	}
}
