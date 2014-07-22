namespace Castle.Zmq
{
	using System;


	/// <summary>
	/// In order to not carry a dependency on a log lib...
	/// </summary>
	public static class LogAdapter
	{
		public static bool LogEnabled;

		static LogAdapter()
		{
			LogAdapter.LogDebugFn = (c,m) => Console.Out.WriteLine("{0}: {1}", c, m);
			LogAdapter.LogErrorFn = (c, m) => Console.Error.WriteLine("{0}: {1}", c, m);
		}

		public static Action<string, string> LogDebugFn { get; set; }
		public static Action<string, string> LogErrorFn { get; set; }

		public static void LogDebug(string context, string message)
		{
			LogDebugFn(context, message);
		}
		public static void LogError(string context, string message)
		{
			LogErrorFn(context, message);
		}
	}
}