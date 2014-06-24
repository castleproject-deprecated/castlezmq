namespace Castle.Facilities.Zmq.Rpc.Tests
{
	using System;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using FluentAssertions;
	using NUnit.Framework;
	using ProtoBuf;

	[TestFixture]
	public class ParameterSerializationTestCase
	{
		[Test]
		public void list_of_types_are_serialized_into_efficient_string_array()
		{
			var types = new[] { typeof(string), typeof(Int16), typeof(int), 
								typeof(DateTime), typeof(Int64), 
								typeof(DateTimeKind), typeof(MyCustomClass) };

			var serialized = Serialization.SerializeParameterTypes(types);
			serialized.Length.Should().Be(types.Length);

			var deserializedTypes = Serialization.DeserializeParameterTypes(serialized);
			deserializedTypes.Length.Should().Be(types.Length);

			deserializedTypes.ShouldAllBeEquivalentTo(types);
		}

		[Test]
		public void With_simple_types_should_serialize_both_ways()
		{
			var types = new[] { typeof(string), typeof(int), typeof(DateTime) };

			var dt = DateTime.Now;
			var buffers = Builder.ParametersToParamTuple(
				new object[]
				{
					"123", 1, dt
				},
				types);

			var args = Builder.ParamTupleToObjects(buffers, types);

			args.Length.Should().Be(3);
			args[0].Should().Be("123");
			args[1].Should().Be(1);
			args[2].Should().Be(dt);
		}

		[Test]
		public void With_other_protocol_should_serialize_both_ways()
		{
			var types = new[] { typeof(MyCustomClass) };

			var buffers = Builder.ParametersToParamTuple(
				new object[]
				{
					new MyCustomClass() { Age = 33, Name = "test" }
				},
				types);

			var args = Builder.ParamTupleToObjects(buffers, types);

			args.Length.Should().Be(1);
			args[0].Should().NotBeNull();
			var my = (MyCustomClass)args[0];
			my.Age.Should().Be(33);
			my.Name.Should().Be("test");
		}

		[ProtoContract]
		public class MyCustomClass
		{
			[ProtoMember(1)]
			public string Name;
			[ProtoMember(2)]
			public int Age;
		}
	}
}