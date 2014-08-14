namespace Castle.Zmq
{
	public sealed class ZmqError
	{
		public ZmqError(int errorCode)
		{
			this.Error = errorCode;
			this.Message = Native.LastErrorString(errorCode);
		}

		public int Error { get; private set; }
		public string Message { get; private set; }

		public override string ToString()
		{
			return this.Error + " - " + this.Message;
		}
	}
}