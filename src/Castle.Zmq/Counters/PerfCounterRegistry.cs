namespace Castle.Zmq.Counters
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	public static class PerfCounterRegistry
	{
		public const string CategoryName = "0MQ";

		private static readonly Dictionary<string, PerformanceCounterType> Registry = new Dictionary<string, PerformanceCounterType>();
		private static readonly Dictionary<string, PerformanceCounter> Counters = new Dictionary<string, PerformanceCounter>();

		public static void Init()
		{
			RegisterAll();
		}

		public static PerformanceCounter Get(string counter)
		{
			return Get(counter, GetInstanceName());
		}

		public static PerformanceCounter Get(string counter, string instance)
		{
			var key = counter + " - " + instance;

			lock (Counters)
				if (!Counters.ContainsKey(key))
				{
					RegisterAll();

					var performanceCounter = new PerformanceCounter
					{
						CategoryName = CategoryName,
						CounterName = counter,
						InstanceName = instance,
						ReadOnly = false,
						InstanceLifetime = PerformanceCounterInstanceLifetime.Process
					};

					performanceCounter.RawValue = 0;

					Counters.Add(key, performanceCounter);
				}

			return Counters[key];
		}

		private static void RegisterAll()
		{
			if (Registry.Count == 0)
			{
				Registry.Add(PerfCounters.NumberOfRequestsReceived, PerformanceCounterType.RateOfCountsPerSecond32);
				Registry.Add(PerfCounters.NumberOfResponseReceived, PerformanceCounterType.RateOfCountsPerSecond32);
				Registry.Add(PerfCounters.NumberOfResponseSent, PerformanceCounterType.RateOfCountsPerSecond32);
				Registry.Add(PerfCounters.NumberOfRequestsSent, PerformanceCounterType.RateOfCountsPerSecond32);
				Registry.Add(PerfCounters.AverageReplyTime, PerformanceCounterType.AverageTimer32);
				Registry.Add(PerfCounters.BaseReplyTime, PerformanceCounterType.AverageBase);
				Registry.Add(PerfCounters.AverageRequestTime, PerformanceCounterType.AverageTimer32);
				Registry.Add(PerfCounters.BaseRequestTime, PerformanceCounterType.AverageBase);
				Registry.Add(PerfCounters.NumberOfCallForwardedToBackend, PerformanceCounterType.RateOfCountsPerSecond32);
				Registry.Add(PerfCounters.NumberOfCallForwardedToFrontend, PerformanceCounterType.RateOfCountsPerSecond32);
			}

			Synchronize();
		}

		private static void Synchronize()
		{
			if (!PerformanceCounterCategory.Exists(CategoryName))
			{
				CreatePerfCounters();
			}
			else
			{
				var category = PerformanceCounterCategory.GetCategories().First(c => c.CategoryName == CategoryName);

				if (!Registry.Keys.Any(category.CounterExists))
				{
					PerformanceCounterCategory.Delete(CategoryName);

					CreatePerfCounters();
				}
			}
		}

		private static void CreatePerfCounters()
		{
			var toCreate = new CounterCreationDataCollection();

			foreach (var entry in Registry)
			{
				var counter = new CounterCreationData { CounterType = entry.Value, CounterName = entry.Key };

				toCreate.Add(counter);
			}

			PerformanceCounterCategory.Create(CategoryName, "0MQ Performance Counters", PerformanceCounterCategoryType.MultiInstance, toCreate);
		}

		private static string GetInstanceName()
		{
			var port = System.Configuration.ConfigurationManager.AppSettings.Get("listening_port")
					   ?? System.Configuration.ConfigurationManager.AppSettings.Get("hosting:app")
					   ?? " - ";

			return Process.GetCurrentProcess().ProcessName + ":" + port;
		}
	}
}
