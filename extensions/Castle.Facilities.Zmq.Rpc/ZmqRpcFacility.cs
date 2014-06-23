namespace Castle.Facilities.Zmq.Rpc
{
	using System;
	using System.Linq;
	using Castle.Facilities.Startable;
	using Castle.Facilities.Zmq.Rpc.Remote;
	using Castle.MicroKernel.Facilities;
	using Castle.MicroKernel.Registration;

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
					Component.For<RemoteEndpointRegistry>(),
					Component.For<ZmqRpcCleaner>(),
					Component.For<RemoteRequestInterceptor>().LifeStyle.Transient
				);
			Kernel.ComponentModelBuilder.AddContributor(new RemoteRequestInspector());

			this._cleaner = this.Kernel.Resolve<ZmqRpcCleaner>();

			_isServer = !(String.IsNullOrEmpty(base.FacilityConfig.Attributes["listen"]));

			// PerfCounters.setIsEnabled ( StringComparer.OrdinalIgnoreCase.Equals("true", base.FacilityConfig.Attributes.["usePerfCounter"]) )

		}

		protected override void Dispose()
		{
			base.Dispose();

			if (_cleaner != null)
			{
				_cleaner.CleanUp();
			}
		}
    }


	public class ZmqRpcCleaner 
	{
		public ZmqRpcCleaner()
		{
		}

		public void CleanUp()
		{
			
		}
	}
}
