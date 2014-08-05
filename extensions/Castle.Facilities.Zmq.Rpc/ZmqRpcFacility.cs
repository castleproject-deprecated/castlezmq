namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using System.Linq;
	using Castle.Facilities.Startable;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using Castle.Facilities.Zmq.Rpc.Local;
	using Castle.Facilities.Zmq.Rpc.Remote;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;
	using Castle.Zmq.Extensions;

	public class ZmqRpcFacility : AbstractFacility
    {
		private bool _isServer;
		private ZmqRpcCleaner _cleaner;

		protected override void Init()
		{
			if (!(this.Kernel.GetFacilities().Any(f => f.GetType() == typeof (StartableFacility))))
			{
				this.Kernel.AddFacility<StartableFacility>();
			}

			this.Kernel.Register(
					Component.For<Castle.Zmq.Context>().Forward<Castle.Zmq.IZmqContext>(),
					Component.For<SerializationStrategy>().ImplementedBy<ProtoBufSerializationStrategy>(),
					Component.For<RemoteEndpointRegistry>(),
					Component.For<RemoteRequestService>(),
					Component.For<RequestPoll>(),
					Component.For<ZmqRpcCleaner>(),
					Component.For<LocalInvocationDispatcher>(),
					Component.For<RemoteRequestInterceptor>().LifeStyle.Transient
				);
			Kernel.ComponentModelBuilder.AddContributor(new RemoteRequestInspector());

			this._isServer = !(String.IsNullOrEmpty(base.FacilityConfig.Attributes["listen"]));

			// PerfCounters.setIsEnabled ( StringComparer.OrdinalIgnoreCase.Equals("true", base.FacilityConfig.Attributes.["usePerfCounter"]) )

			if (this._isServer)
			{
				this.SetUpServer();
			}

			if (base.FacilityConfig.Children["endpoints"] != null)
			{
				this.SetUpClient();
			}

			this._cleaner = this.Kernel.Resolve<ZmqRpcCleaner>();
		}

		protected override void Dispose()
		{
			base.Dispose();

			if (_cleaner != null)
			{
				_cleaner.CleanUp();
			}
		}

		private void SetUpClient()
		{
			var router = base.Kernel.Resolve<RemoteEndpointRegistry>();
			router.ParseEndpoints(base.FacilityConfig.Children["endpoints"]);
		}

		private void SetUpServer()
		{
			var listeningEndpoint = base.FacilityConfig.Attributes["listen"];
			var workers = base.FacilityConfig.Attributes["workers"] ?? "30";

			if (listeningEndpoint.IndexOf("://", StringComparison.Ordinal) == -1)
			{
				listeningEndpoint = "tcp://" + listeningEndpoint;
			}

			this.Kernel.Register(
						Component.For<RemoteRequestListener>().Parameters(
								Parameter.ForKey("endpoint").Eq(listeningEndpoint),
                                Parameter.ForKey("workers").Eq(workers))
				);
		}
    }
}
