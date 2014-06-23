namespace Castle.Facilities.Zmq.Rpc
{
	using System.Linq;
	using Castle.Core;
	using Castle.MicroKernel;
	using Castle.MicroKernel.ModelBuilder.Inspectors;
	using Castle.Zmq.Rpc;

	public class RemoteRequestInspector : MethodMetaInspector
	{
		protected override string ObtainNodeName()
		{
			return "remote-interceptor";
		}

		public override void ProcessModel(IKernel kernel, ComponentModel model)
		{
			if (model.Services.Any(s => s.IsDefined(typeof (RemoteServiceAttribute), false)))
			{
				this.AddInterceptor(model);
			}
		}

		private void AddInterceptor(ComponentModel model)
		{
			model.Dependencies.Add(new DependencyModel(this.ObtainNodeName(), typeof (RemoteRequestInterceptor), false));
			model.Interceptors.Add(new InterceptorReference(typeof(RemoteRequestInterceptor)));
		}
	}
}