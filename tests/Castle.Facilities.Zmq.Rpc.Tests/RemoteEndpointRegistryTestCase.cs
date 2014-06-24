namespace Castle.Facilities.Zmq.Rpc.Tests
{
	using System;
	using Castle.Core.Configuration;
	using Castle.Facilities.Zmq.Rpc.Remote;
	using NUnit.Framework;

	[TestFixture]
	public class RemoteEndpointRegistryTestCase
	{
		[Test]
		public void ParseEndpoints_for_null_config_throws()
		{
			Assert.Throws<ArgumentNullException>(() =>
			{
				var reg = new RemoteEndpointRegistry();
				reg.ParseEndpoints(null);
			});
		}

		[Test]
		public void ParseEndpoints_for_nonempty_but_nochildren_config()
		{
			var reg = new RemoteEndpointRegistry();
			reg.ParseEndpoints(new MutableConfiguration("entries"));
		}

		[Test]
		public void ParseEndpoints_reflects_entries()
		{
			var config = new MutableConfiguration("entries");
			config.CreateChild("item").Attribute("assembly", "asm1").Attribute("endpoint", "end");

			var reg = new RemoteEndpointRegistry();
			reg.ParseEndpoints(config);

			Assert.AreEqual(1, reg.EntriesCount);
		}
	}
}
