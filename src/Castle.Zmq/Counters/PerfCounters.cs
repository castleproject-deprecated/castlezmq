namespace Castle.Zmq.Counters
{
	public static class PerfCounters
	{
		public const string NumberOfRequestsReceived = "# of Requests Received / sec";
		public const string NumberOfResponseSent = "# of Response Sent / sec";

		public const string NumberOfRequestsSent = "# of Response Received / sec";
		public const string NumberOfResponseReceived = "# of Requests Sent / sec";

		public const string AverageReplyTime = "Average Reply Time";
		public const string AverageRequestTime = "Average Request Time";

		public const string NumberOfCallForwardedToFrontend = "# of Forwarded To Frontend / sec";
		public const string NumberOfCallForwardedToBackend = "# of Forwarded To Backend / sec";

		public const string BaseReplyTime = "Base Average Reply Time";
		public const string BaseRequestTime = "Base Average Request Time";

	}
}