namespace Castle.Facilities.Zmq.Rpc.Tests
{
	using System;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using Castle.Facilities.Zmq.Rpc.Local;
	using Castle.Zmq.Rpc.Model;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class LocalInvocationDispatcherTestCase
	{
		const string serviceName = "serv";

		private LocalInvocationDispatcher _dispatcher;
		private StubService _service;

		[SetUp]
		public void Init()
		{
			_service = new StubService();
			_dispatcher = new LocalInvocationDispatcher(service =>
			{
				if (service == serviceName) return _service;
				throw new Exception("service not found");
			}, new ProtoBufSerializationStrategy());
		}

		[Test]
		public void dispatches_to_method_with_no_overloads()
		{
			var result = _dispatcher.Dispatch(serviceName, "Operation1", new ParamTuple[0], new string[0]);

			_service.Operation1Invoked.Should().BeTrue();

			Assert.IsNotNull(result);

			Assert.IsNull(result.Item1);
			Assert.AreEqual(typeof(void), result.Item2);
		}

		[Test]
		public void dispatches_to_method_with_overloads()
		{
			var p1 = Serialization.BuildParamTuple(typeof (string), "test");

			var result = _dispatcher.Dispatch(serviceName, "Operation2", new ParamTuple[] { p1 }, new[] { "str" });

			_service.Operation2Invoked.Should().BeTrue();

			Assert.IsNotNull(result);

			Assert.IsNull(result.Item1);
			Assert.AreEqual(typeof(void), result.Item2);
		}

		[Test]
		public void dispatches_to_method_with_return()
		{
			var p1 = Serialization.BuildParamTuple(typeof(int), 10);
			var p2 = Serialization.BuildParamTuple(typeof(int), 20);

			var result = _dispatcher.Dispatch(serviceName, "Add", new[] { p1, p2 }, new[] { "int32", "int32" });

			_service.AddInvoked.Should().BeTrue();

			Assert.IsNotNull(result);

			result.Item1.Should().Be(30);
			Assert.AreEqual(typeof(int), result.Item2);
		}

		class StubService
		{
			public bool Operation1Invoked { get; private set; }
			public bool Operation2Invoked { get; private set; }
			public bool AddInvoked { get; private set; }

			public void Operation1()
			{
				this.Operation1Invoked = true;
			}

			public void Operation2(string c)
			{
				this.Operation2Invoked = true;
			}
			public void Operation2(int s)
			{
				this.Operation2Invoked = true;
			}

			public int Add(int x, int y)
			{
				this.AddInvoked = true;
				return x + y;
			}
		}
	}
}
