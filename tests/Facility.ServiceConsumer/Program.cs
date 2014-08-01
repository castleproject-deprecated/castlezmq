namespace Facility.ServiceConsumer
{
	using System;
	using System.IO;
	using System.Threading;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;
	using Facility.TestCommon;

	class Program
	{
		private static WindsorContainer _containerClient;
		private static bool _stopping = false;
		private static ManualResetEventSlim _proceedEvent = new ManualResetEventSlim(false);

		static void Main(string[] args)
		{
			// Castle.Zmq.Context.EnsureZmqLibrary();
			Castle.Zmq.LogAdapter.LogEnabled = true;
			Castle.Zmq.LogAdapter.LogDebugFn = (scope, msg) => Console.WriteLine("Debug {0}: {1}", scope, msg);
			Castle.Zmq.LogAdapter.LogErrorFn = (scope, msg) => Console.WriteLine("Error {0}: {1}", scope, msg);

			_containerClient = new WindsorContainer(new XmlInterpreter("config_client.config"));

			_containerClient.Register(Component.For<IRemoteServ1>());

			Console.WriteLine("Started. Initial request...");

			// Do initial request to see if path is clear


			var service = _containerClient.Resolve<IRemoteServ1>();
			InvokeBatch(service);


			Console.WriteLine("Performing firehose requests");

			for (var i = 0; i < 50; i++)
			{
				var t = new Thread(TestRemoteService) {IsBackground = true};
				t.Start();
			}

			// Release the horses
			_proceedEvent.Set();

			Console.CancelKeyPress += (sender, eventArgs) =>
			{
				_stopping = true;

				Thread.Sleep(1000);

				_containerClient.Dispose();
			};

			Thread.CurrentThread.Join();
		}

		private static void TestRemoteService()
		{
			_proceedEvent.Wait();

			try
			{
				var service = _containerClient.Resolve<IRemoteServ1>();

				while (!_stopping)
				{
					InvokeBatch(service);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error " + ex);
			}
		}

		private static void InvokeBatch(IRemoteServ1 service)
		{
//			try
//			{
//				service.DoSomethingWrong();
//				Assert.Fail("Expecting exception here");
//			}
//			catch (Exception ex)
//			{
//				Assert.AreEqual("Remote server or invoker threw Exception with message simple message", ex.Message);
//			}
//
			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			// 1000
			for (var i = 0; i < 1000; i++)
			{
				// Console.WriteLine("new batch ");

				service.NoParamsOrReturn();
				service.JustParams("1");
//				Assert.IsTrue(service.JustReturn().Equals("abc"));
				service.ParamsWithStruct(new MyCustomStruct() { Name = "1", Age = 30 });
				service.ParamsWithCustomType1(new Impl1() { });
				service.ParamsWithCustomType2(new Contract1Impl() { Name = "2", Age = 31 });
				service.ParamsAndReturn(Guid.NewGuid(), "", 1, DateTime.Now, 102.2m, FileAccess.ReadWrite, 1, 2, 3.0f, 4.0);
				service.WithInheritanceParam(new Derived1() { Something = 10, DerivedProp1 = 20 });

				var b = service.WithInheritanceRet();
//				Assert.IsNotNull(b);
//				Assert.IsInstanceOf(typeof(Derived2), b);
//				Assert.AreEqual(10, (b as Derived2).Something);
//				Assert.AreEqual("test", (b as Derived2).DerivedProp2);

				var enu = service.UsingEnumerators();
//				Assert.IsNotNull(enu);
//				Assert.AreEqual(2, enu.Count());

				var array = service.UsingArray();
//				Assert.IsNotNull(array);
//				Assert.AreEqual(2, array.Length);

				service.ReturningNull1();
				service.ReturningNull2();
			}

//			Console.WriteLine(".");

			watch.Stop();

			Console.WriteLine(". " + watch.ElapsedMilliseconds);
		}
	}
}
