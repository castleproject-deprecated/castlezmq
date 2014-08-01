namespace Facility.RemoteServiceHost
{
	using System;
	using System.Threading;
	using Castle.MicroKernel.Registration;
	using Castle.Windsor;
	using Castle.Windsor.Configuration.Interpreters;
	using Facility.TestCommon;

	class Program
	{
		private static WindsorContainer _containerServer;

		static void Main(string[] args)
		{
			Console.WriteLine("Starting");

			Castle.Zmq.Context.EnsureZmqLibrary();
			Castle.Zmq.LogAdapter.LogEnabled = true;
			Castle.Zmq.LogAdapter.LogDebugFn = (scope, msg) => Console.WriteLine("Debug {0}: {1}", scope, msg);
			Castle.Zmq.LogAdapter.LogErrorFn = (scope, msg) => Console.WriteLine("Error {0}: {1}", scope, msg);


			_containerServer = new WindsorContainer(new XmlInterpreter("config_server.config"));

			_containerServer.Register(Component.For<IRemoteServ1>().ImplementedBy<RemoteServImpl>());


			Console.CancelKeyPress += (sender, eventArgs) =>
			{
				Console.WriteLine("Disposing");

				_containerServer.Dispose();
			};

			Console.WriteLine("Started");
			Thread.CurrentThread.Join();
		}
	}
}
