namespace Castle.Facilities.Zmq.Rpc.Remote
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Runtime.Remoting.Messaging;
	using Castle.Core.Configuration;

	public class RemoteEndpointRegistry 
	{
		private readonly Dictionary<string,string> _endpoints = new Dictionary<string, string>(); 

		public int EntriesCount {get { return _endpoints.Count; }}

		public void ParseEndpoints(IConfiguration config)
		{
			if (config == null) throw new ArgumentNullException("config");

			foreach (var child in config.Children)
			{
				var asm = child.Attributes["assembly"];
				var endpoint = child.Attributes["address"] ?? child.Attributes["endpoint"];

				if (asm == null) throw new ArgumentException("config is missing 'assembly' attribute");
				if (endpoint == null) throw new ArgumentException("config is missing 'endpoint' attribute");

				if (endpoint.IndexOf("://", StringComparison.Ordinal) == -1)
				{
					// defaults to tcp
					endpoint = "tcp://" + endpoint;
				}

				this._endpoints[asm] = endpoint;
			}
		}

		public void ReRoute(string asm, string endpoint)
		{
			if (asm == null) throw new ArgumentNullException("asm");
			if (endpoint == null) throw new ArgumentNullException("endpoint");

			this._endpoints[asm] = endpoint;
		}

		public string GetEndpoint(Assembly asm)
		{
			// TODO: isnt this faster than getName)?
			var full = asm.FullName;

			return GetEndpoint(asm.GetName().Name);
		}

		public string GetEndpoint(string name)
		{
			var overriden = (string) CallContext.GetData("0mq.facility.endpoint");

			if (!string.IsNullOrEmpty(overriden))
			{
				return overriden;
			}

			string endpoint;
			if (!_endpoints.TryGetValue(name, out endpoint))
			{
				throw new InvalidOperationException("No endpoint configured for " + name);
			}

			return endpoint;
		}
	}


	/// <summary>
	/// Overrides an endpoint
	/// </summary>
	public class AlternativeRouteContext : IDisposable
	{
		public AlternativeRouteContext(string route)
		{
			if (route == null) throw new ArgumentNullException("route");
			CallContext.SetData("0mq.facility.endpoint", route);
		}

		public void Dispose()
		{
			CallContext.SetData("0mq.facility.endpoint", null);
		}

		public static IDisposable For(string route)
		{
			return new AlternativeRouteContext(route);
		}
	}
}
