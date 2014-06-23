namespace RpcFacilityStressTest
{
	using System;
	using System.IO;
	using System.Linq;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;
	using NUnit.Framework;

	class Program
	{
		private static WindsorContainer _containerClient;
		private static WindsorContainer _containerServer;

		static void Main(string[] args)
		{
			_containerClient = new WindsorContainer(new XmlInterpreter("config_client.config"));
			_containerServer = new WindsorContainer(new XmlInterpreter("config_server.config"));

			_containerServer.Register(Component.For<IRemoteServ1>().ImplementedBy<RemoteServImpl>());
			_containerClient.Register(Component.For<IRemoteServ1>());

			try
			{
				var service = _containerClient.Resolve<IRemoteServ1>();

				InvokeBatch(service);
			}
			finally
			{
				Console.WriteLine("Disposing client");
				_containerClient.Dispose();

				Console.WriteLine("Disposing server");
				_containerServer.Dispose();

				Console.WriteLine("Disposed");
			}
		}

		private static void InvokeBatch(IRemoteServ1 service)
		{
			try
			{
				service.DoSomethingWrong();
				Assert.Fail("Expecting exception here");
			}
			catch (Exception ex)
			{
				Assert.AreEqual("Remote server threw Exception with message simple message", ex.Message);
			}

			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			// 1000
			for (var i = 0; i < 100; i++)
			{
				// Console.WriteLine("new batch ");

				service.NoParamsOrReturn();
				service.JustParams("1");
				service.JustReturn().Equals("abc");
				service.ParamsWithStruct(new MyCustomStruct() { Name = "1", Age = 30 });
				service.ParamsWithCustomType1(new Impl1() { });
				service.ParamsWithCustomType2(new Contract1Impl() { Name = "2", Age = 31 });
				service.ParamsAndReturn(Guid.NewGuid(), "", 1, DateTime.Now, 102.2m, FileAccess.ReadWrite, 1, 2, 3.0f, 4.0);
				service.WithInheritanceParam(new Derived1() { Something = 10, DerivedProp1 = 20 });

				var b = service.WithInheritanceRet();
				Assert.IsNotNull(b);
				Assert.IsInstanceOf(typeof(Derived2), b);
				Assert.AreEqual(10, (b as Derived2).Something);
				Assert.AreEqual("test", (b as Derived2).DerivedProp2);

				var enu = service.UsingEnumerators();
				Assert.IsNotNull(enu);
				Assert.AreEqual(2, enu.Count());

				var array = service.UsingArray();
				Assert.IsNotNull(array);
				Assert.AreEqual(2, array.Length);

				
			}

			watch.Stop();

			Console.WriteLine("took " + watch.ElapsedMilliseconds);
		}
	}
}
