namespace Castle.Facilities.Zmq.Rpc.Tests
{
	using Castle.Facilities.Zmq.Rpc.Internal;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class ArraySerializationTestCase
	{
		[Test]
		public void Simplest_case_works()
		{
			var buffer = Serialization.serialize_array(new[] { "1", "2", "3" });

			var array = Serialization.deserialize_array(typeof(string[]), buffer);
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
			var buffer = Serialization.serialize_array(new[] { 1, 2, 3 });

			var array = Serialization.deserialize_array(typeof(int[]), buffer);
			array.Should().BeOfType<int[]>();
			//			var ints = (int[])array;
			//			ints.Length.Should().Be(3);
			//			ints[0].Should().Be(1);
			//			ints[1].Should().Be(2);
			//			ints[2].Should().Be(3);
		}

		[Test]
		public void Protocol_case_works()
		{
			var buffer = Serialization.serialize_array(new[]
			{
				new MyCustomClass() { Age = 1, Name = "1" },
				new MyCustomClass() { Age = 2, Name = "2" }
			});

			var array = Serialization.deserialize_array(typeof(MyCustomClass[]), buffer);
			array.Should().BeOfType<MyCustomClass[]>();
			var myCustomClasses = (MyCustomClass[])array;
			myCustomClasses.Length.Should().Be(2);
			myCustomClasses[0].Age.Should().Be(1);
			myCustomClasses[1].Age.Should().Be(2);
			myCustomClasses[0].Name.Should().Be("1");
			myCustomClasses[1].Name.Should().Be("2");
		}

		[Test]
		public void Protocol_pseudo_inheritance_case_works()
		{
			var buffer = Serialization.serialize_array(new Base[]
			{
				new Derived1() { Something = 2, DerivedProp1 = 1 },
				new Derived2() { Something = 10, DerivedProp2 = "1" },
				new Derived1() { Something = 3, DerivedProp1 = 1000 },
			});

			var array = Serialization.deserialize_array(typeof(Base[]), buffer);
			array.Should().BeOfType<Base[]>();
			var myCustomClasses = (Base[])array;
			myCustomClasses.Length.Should().Be(3);
			myCustomClasses[0].Should().BeOfType<Derived1>();
			myCustomClasses[1].Should().BeOfType<Derived2>();
			myCustomClasses[2].Should().BeOfType<Derived1>();
		}
	}
}