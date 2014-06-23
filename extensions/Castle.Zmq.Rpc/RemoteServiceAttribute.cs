namespace Castle.Zmq.Rpc
{
	using System;


	/// <summary>
	/// Indicates that an interface implementation is over the wire
	/// Signals to the facility that it should be proxied and invocations
	/// be done through Req/Rep using our own homebrew protocol
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public class RemoteServiceAttribute : Attribute
	{
	}
}
