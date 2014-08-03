namespace Castle.Facilities.Zmq.Rpc.Tests
{
	using System;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using Castle.Facilities.Zmq.Rpc.Remote;
	using Castle.Zmq.Extensions;
	using Castle.Zmq.Rpc.Model;
	using Castle.Zmq.Stubs;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class RemoteRequestServiceTestCase
	{
		private const string TestHostName = "host";
		private const string TestService = "service";

		private StubContext _context;
		private RemoteEndpointRegistry _registry;
		private SerializationStrategy _serialization;

		[SetUp]
		public void Init()
		{
			_registry = new RemoteEndpointRegistry();
			_serialization = new ProtoBufSerializationStrategy();

			_registry.ReRoute(TestHostName, "endpoint1");
		}

		[Test]
		public void Invoke_sends_request_to_right_endpoint()
		{
			StubSocket socket = null;

			_context = new StubContext(type =>
			{
				socket = new StubSocket(type);
				socket.ToRecv.Add( Builder.SerializeResponse(new ResponseMessage()) );
				return socket;
			});


			var req = new RemoteRequestService(_context, _registry, _serialization, new RequestPoll(_context));

			var result = req.Invoke(TestHostName, TestService, "method", 
									new object[0], Type.EmptyTypes, typeof (void));

			socket.ConnectedToEndpoint.Should().Be("endpoint1"); 
			result.Should().BeNull();
		}

		[Test]
		public void Invoke_sends_request_and_restore_result()
		{
			StubSocket socket = null;

			_context = new StubContext(type =>
			{
				socket = new StubSocket(type);
				socket.ToRecv.Add(Builder.SerializeResponse( Builder.BuildResponse(new Tuple<object, Type>("hello", typeof(string))) ));
				return socket;
			});


			var req = new RemoteRequestService(_context, _registry, _serialization, new RequestPoll(_context));

			var result = req.Invoke(TestHostName, TestService, "method",
									new object[0], Type.EmptyTypes, typeof(string));

			result.Should().Be("hello");
		}

		[Test]
		public void Invoke_sends_request_and_restore_exception()
		{
			StubSocket socket = null;

			_context = new StubContext(type =>
			{
				socket = new StubSocket(type);
				socket.ToRecv.Add(Builder.SerializeResponse(Builder.BuildResponse(new Exception("for testing"))));
				return socket;
			});

			var req = new RemoteRequestService(_context, _registry, _serialization, new RequestPoll(_context));

			Assert.Throws<Exception>(() =>
			{
				var result = req.Invoke(TestHostName, TestService, "method",
					new object[0], Type.EmptyTypes, typeof (string));

			}).Message.Should().Be("Remote server or invoker threw Exception with message for testing");
		}
	}
}
