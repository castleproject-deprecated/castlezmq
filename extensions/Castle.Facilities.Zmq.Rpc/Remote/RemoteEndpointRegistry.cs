namespace Castle.Facilities.Zmq.Rpc.Remote
{
	using System;
	using System.Reflection;
	using Castle.Core.Configuration;

	public class RemoteEndpointRegistry 
	{
		public RemoteEndpointRegistry(IConfiguration config)
		{
		}

		public string GetEndpoint(Assembly asm)
		{
			throw new NotImplementedException();
		}
	}
}
