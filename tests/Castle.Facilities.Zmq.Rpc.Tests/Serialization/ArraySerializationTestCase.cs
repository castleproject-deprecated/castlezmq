namespace Castle.Facilities.Zmq.Rpc.Tests
{
	using System.Linq;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using FluentAssertions;
	using NUnit.Framework;
	using ProtoBuf;

	[TestFixture]
	public class ArraySerializationTestCase
	{
		[Test]
		public void Simplest_case_works()
		{
			var buffer = Serialization.SerializeArray(new[] { "1", "2", "3" });

			var array = Serialization.DeserializeArray(buffer, typeof(string[]));
			array.Should().BeOfType<string[]>();
			var strings = (string[])array;
			strings.Length.Should().Be(3);
			strings[0].Should().Be("1");
			strings[1].Should().Be("2");
			strings[2].Should().Be("3");
		}

		[Test]
		public void Int32_case_works()
		{
			var buffer = Serialization.SerializeArray(new[] { 1, 2, 3 });

			var array = Serialization.DeserializeArray(buffer, typeof(int[]));
			array.Should().BeOfType<int[]>();
			var list = array;
			list.Count.Should().Be(3);
			list[0].Should().Be(1);
			list[1].Should().Be(2);
			list[2].Should().Be(3);
		}

		[Test]
		public void Protocol_case_works()
		{
			var buffer = Serialization.SerializeArray(new[]
			{
				new ParameterSerializationTestCase.MyCustomClass() { Age = 1, Name = "1" },
				new ParameterSerializationTestCase.MyCustomClass() { Age = 2, Name = "2" }
			});

			var array = Serialization.DeserializeArray(buffer, typeof(ParameterSerializationTestCase.MyCustomClass[]));
			array.Should().BeOfType<ParameterSerializationTestCase.MyCustomClass[]>();
			var myCustomClasses = (ParameterSerializationTestCase.MyCustomClass[])array;
			myCustomClasses.Length.Should().Be(2);
			myCustomClasses[0].Age.Should().Be(1);
			myCustomClasses[1].Age.Should().Be(2);
			myCustomClasses[0].Name.Should().Be("1");
			myCustomClasses[1].Name.Should().Be("2");
		}

		[Test]
		public void Protocol_pseudo_inheritance_case_works()
		{
			var buffer = Serialization.SerializeArray(new Base[]
			{
				new Derived1() { Something = 2, DerivedProp1 = 1 },
				new Derived2() { Something = 10, DerivedProp2 = "1" },
				new Derived1() { Something = 3, DerivedProp1 = 1000 },
			});

			var array = Serialization.DeserializeArray(buffer, typeof(Base[]));
			array.Should().BeOfType<Base[]>();
			var myCustomClasses = (Base[])array;
			myCustomClasses.Length.Should().Be(3);
			myCustomClasses[0].Should().BeOfType<Derived1>();
			myCustomClasses[1].Should().BeOfType<Derived2>();
			myCustomClasses[2].Should().BeOfType<Derived1>();
		}

		[ProtoContract]
		[ProtoInclude(1, typeof(Derived1))]
		[ProtoInclude(2, typeof(Derived2))]
		public class Base
		{
			[ProtoMember(10)]
			public int Something { get; set; }
		}
		[ProtoContract]
		public class Derived1 : Base
		{
			[ProtoMember(20)]
			public int DerivedProp1 { get; set; }
		}
		[ProtoContract]
		public class Derived2 : Base
		{
			[ProtoMember(30)]
			public string DerivedProp2 { get; set; }
		}
	}
}