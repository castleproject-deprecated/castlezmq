namespace Castle.Facilities.Zmq.Rpc.Tests
{
	using System;
	using Castle.Facilities.Zmq.Rpc.Local;
	using Castle.Zmq.Rpc.Model;
	using NUnit.Framework;

	[TestFixture]
	public class LocalInvocationDispatcherTestCase
	{
		[Test]
		public void Dis()
		{
			var serviceName = "serv";

			var dispatcher = new LocalInvocationDispatcher(service =>
			{
				if (service == serviceName) return new StubService();
				throw new Exception("service not found");
			});

			var result = dispatcher.Dispatch(serviceName, "Operation1", new ParamTuple[0], new string[0]);

			Assert.IsNotNull(result);

			Assert.IsNull(result.Item1);
			Assert.AreEqual(typeof(void), result.Item2);
		}


		class StubService
		{
			public void Operation1()
			{
				
			}
		}
	}
}
