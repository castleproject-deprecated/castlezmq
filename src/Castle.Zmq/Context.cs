namespace Castle.Zmq
{
	using System;
	using System.Collections;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Threading;


	/// <summary>
	/// Holds a zmq context. See http://api.zeromq.org/4-0:zmq-ctx-new
	/// </summary>
	public class Context : IZmqContext, IDisposable
	{
		private volatile bool _disposed;
		
		internal readonly IntPtr _contextPtr;

		public const int DefaultIoThreads = 1;
		public const int DefaultMaxSockets = 1024;

	    /// <summary>
		/// Creates a new context
		/// </summary>
		/// <param name="ioThreads">How many dedicated threds for IO operations</param>
		/// <param name="maxSockets">Max sockets allowed for this context</param>
		public Context(int ioThreads, int maxSockets)
		{
			if (ioThreads < 0) throw new ArgumentException("ioThreads can't be less than zero", "ioThreads");
			if (maxSockets < 1) throw new ArgumentException("maxSockets can't be less than one", "maxSockets");

			this._contextPtr = Native.Context.zmq_ctx_new();

			if (this._contextPtr == IntPtr.Zero)
			{
				throw new ZmqException("Could not allocate a Zmq Context");
			}

			// Just in case to avoid memory leaks
			if (ioThreads != DefaultIoThreads || maxSockets != DefaultMaxSockets)
			{
				InternalConfigureContext(ioThreads, maxSockets);
			} 
		}

		public Context() :this(DefaultIoThreads, DefaultMaxSockets)
		{
		}

		~Context()
		{
			InternalDispose(false);
		}

		public IZmqSocket CreateSocket(SocketType type)
		{
			return new Socket(this, type);
		}

		public void Dispose()
		{
			this.InternalDispose(true);
		}

		public static void EnsureZmqLibrary()
		{
			IntPtr libPtr = Native.LoadLibrary("libzmq");
			if (libPtr == IntPtr.Zero)
			{
				LoadEmbeddedLibary();
			}
		}

#if DEBUG

		public readonly ConcurrentDictionary<Socket, string> _socket2Creation = new ConcurrentDictionary<Socket, string>(); 

		public void Track(Socket socket)
		{
			var stackTrace = new StackTrace();

			_socket2Creation[socket] = stackTrace.ToString();
		}

		public void Untrack(Socket socket)
		{
			string dummy;
			_socket2Creation.TryRemove(socket, out dummy);
		}

		public IEnumerable<Tuple<Socket, string>> GetTrackedSockets()
		{
			return _socket2Creation
				.Select(pair => Tuple.Create(pair.Key, pair.Value))
				.ToArray();
		}

#endif

		private static void LoadEmbeddedLibary()
		{
			var bitnessPath = String.Format("x{0}", Environment.Is64BitProcess ? 64 : 86);

			var asm = typeof(Context).Assembly;
			string resourceName = String.Format(
				"Castle.Zmq.Native.lib.{0}.libzmq.dll.gz",
				bitnessPath);

			var dir = String.Format("castle-zmq-{0}", typeof(Context).Assembly.GetName().Version);

			dir = Path.Combine(Path.GetTempPath(), dir);
			dir = Path.Combine(dir, bitnessPath);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			var tempFile = Path.Combine(dir, "libzmq.dll");

			// Try to delete and recreate, in case of file corruption
			if (File.Exists(tempFile))
			{
				try
				{
					File.Delete(tempFile);
				}
				catch { } // Might be in use by another process; likely not corrupted
			}

			if (!File.Exists(tempFile)) // if delete was successful, create it
			{
				using (var stream = asm.GetManifestResourceStream(resourceName))
				using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
				using (var file = File.Create(tempFile))
				{
					gzip.CopyTo(file);
					file.Flush(true);
				}
			}

			IntPtr libPtr = Native.LoadLibrary(tempFile);
			if (libPtr == IntPtr.Zero)
			{
				throw new InvalidOperationException("Unable to load libzmq " + (Environment.Is64BitProcess ? "x64" : "win32") + " from " + tempFile);
			}
		}

		private void InternalConfigureContext(int ioThreads, int maxSockets)
		{
			try
			{
				if (ioThreads != DefaultIoThreads)
				{
					var res = Native.Context.zmq_ctx_set(this._contextPtr, Native.Context.IO_THREADS, ioThreads);
					if (res == Native.ErrorCode) Native.ThrowZmqError();
				}
				if (maxSockets != DefaultMaxSockets)
				{
					var res = Native.Context.zmq_ctx_set(this._contextPtr, Native.Context.MAX_SOCKETS, maxSockets);
					if (res == Native.ErrorCode) Native.ThrowZmqError();
				}
			}
			catch (Exception)
			{
				this.InternalDispose(true);
				throw;
			}
		}

		private void InternalDispose(bool isDispose)
		{
			if (this._disposed) return;

			if (isDispose)
			{
				GC.SuppressFinalize(this);
			}

			this._disposed = true;

			if (this._contextPtr != IntPtr.Zero)
			{
				Native.Context.zmq_ctx_shutdown(this._contextPtr); // discard any error

#if DEBUG
				var message = this.GetTrackedSockets()
					.Aggregate("", (prev, tuple) => prev + "Socket " + tuple.Item1.SocketType + " at " + tuple.Item2 + Environment.NewLine);

				System.Diagnostics.Trace.TraceError("Socket tracking: " + message);
				System.Diagnostics.Debug.WriteLine("Socket tracking: " + message);
				if (LogAdapter.LogEnabled)
				{
					LogAdapter.LogError("Context", "Socket tracking: " + message);
				}

				var t = new System.Threading.Thread(() =>
				{
					Thread.Sleep(2000);

					var message2 = 
						this.GetTrackedSockets()
							.Aggregate("", (prev, tuple) => prev + "Socket " + tuple.Item1.SocketType + " at " + tuple.Item2 + Environment.NewLine);

					System.Diagnostics.Trace.TraceError("Socket tracking: " + message2);
					System.Diagnostics.Debug.WriteLine("Socket tracking: " + message2);
					if (LogAdapter.LogEnabled)
					{
						LogAdapter.LogError("Context", "**** STILL Hanging **** - Socket tracking: " + message2);
					}
				});
				t.Start();
#endif

				var error = Native.Context.zmq_ctx_term(this._contextPtr);
				if (error == Native.ErrorCode)
				{
					// Not good, but we can't throw an exception in the Dispose
					var msg = "Error disposing context: " + Native.LastErrorString();
					System.Diagnostics.Trace.TraceError(msg);
					System.Diagnostics.Debug.WriteLine(msg);

					if (LogAdapter.LogEnabled)
					{
						LogAdapter.LogError("Context", msg);
					}
				}
			}
		}

		private static bool? _isMono;
		internal static bool IsMono
		{
			get
			{
				if (!_isMono.HasValue)
				{
					_isMono = (Type.GetType("Mono.Runtime", false) != null);
				}
				return _isMono.Value;
			}
		}
	}
}
