namespace Castle.Zmq.Tests
{
	using NUnit.Framework;

	public abstract class BaseContextSetUp
	{
		protected Context Context { get; private set; }

		[SetUp]
		public void Init()
		{
			this.Context = new Context();
		}

		[TearDown]
		public void End()
		{
			if (this.Context != null)
			{
				this.Context.Dispose();
				this.Context = null;
			}
		}
	}
}