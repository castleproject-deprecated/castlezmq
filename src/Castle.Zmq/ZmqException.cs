namespace Castle.Zmq
{
	using System;
	using System.Runtime.Serialization;

	[Serializable]
	public class ZmqException : Exception
	{
		private readonly int _error;

		public ZmqException(string message) : base(message)
		{
		}

		public ZmqException(string message, int error) : base(message)
		{
			this._error = error;
		}

		public ZmqException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ZmqException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public int ZmqErrorCode { get { return _error; } }
	}
}