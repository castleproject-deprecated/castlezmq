namespace Castle.Zmq.Extensions
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;


	/// <summary>
	/// Mitigates potential SO socket creation/destruction 
	/// limits (exhaustion) by keeping sockets open
	/// </summary>
	public class RequestPoll : IDisposable
	{
		class EndpointPoll
		{
			internal ConcurrentQueue<IZmqSocket> SocketsQueue = new ConcurrentQueue<IZmqSocket>();
//			internal bool monitored;
//			internal MonitoredSocket monitor;
//			internal object locker = new object();
		}

		private readonly IZmqContext _context;
		private readonly ConcurrentDictionary<string, EndpointPoll> _endpoint2Sockets;
		private readonly HashSet<IZmqSocket> _socketsInUse; 
		private volatile bool _disposed;

		public RequestPoll(IZmqContext context)
		{
			this._context = context;
			this._endpoint2Sockets = new ConcurrentDictionary<string, EndpointPoll>(StringComparer.InvariantCultureIgnoreCase);
			this._socketsInUse = new HashSet<IZmqSocket>(); 
		}

		~RequestPoll()
		{
			InternalDispose(false);
		}

		public IZmqSocket Get(string endpoint)
		{
			if (_disposed) throw new ObjectDisposedException("RequestPoll");
			if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

			var endpointPoll = _endpoint2Sockets.GetOrAdd(endpoint, _ => new EndpointPoll());
			var socketQueue = endpointPoll.SocketsQueue;

			IZmqSocket socket;
			if (!socketQueue.TryDequeue(out socket))
			{
				socket = this._context.CreateSocket(SocketType.Req);

//				var isMonitoring = endpointPoll.monitored;
//				if (!isMonitoring)
//				{
//					var locker = endpointPoll.locker;
//					lock (locker)
//					{
//						isMonitoring = endpointPoll.monitored;
//						if (!isMonitoring)
//						{
//							var monitor = new MonitoredSocket(_context, socket);
//							
//							endpointPoll.monitor = monitor;
//							endpointPoll.monitored = true;
//
//							monitor.Error += (zmqSocket, events, arg3) => InvalidatePoll(endpoint, "Monitor: error  " + arg3.ToString() + " - " + events);
//							monitor.Disconnected += (zmqSocket, events, arg3) => InvalidatePoll(endpoint, "Monitor: disconnected " + arg3 + " - " + events);
//							monitor.Connected += (zmqSocket, events, arg3) =>
//							{
//								LogAdapter.LogDebug("RequestPoll", "Connected " + events + "  " + arg3);
//							};
//						}						
//					}
//				}

				socket.Connect(endpoint);
			}
			else
			{
				// Ensure the buffer is clean
				while (socket.HasMoreToRecv())
				{
					var dummy = socket.Recv(noWait: true);
				}
			}

			// track the sockets in use
			lock(this._socketsInUse)
				this._socketsInUse.Add(socket);

			return socket;
		}

		public void Return(IZmqSocket socket, string endpoint, bool inError)
		{
			if (socket == null) throw new ArgumentNullException("socket");
			if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

			var endpointPoll = _endpoint2Sockets.GetOrAdd(endpoint, _ => new EndpointPoll());

			if (_disposed || inError)
			{
//				if (endpointPoll.monitor != null && endpointPoll.monitor.Socket == socket)
//				{
//					endpointPoll.monitor.Dispose();
//					endpointPoll.monitored = false;
//				}

				socket.Dispose();
			}

			this.Untrack(socket);

			if (_disposed || inError)
			{
				return;
			}

			var socketQueue = endpointPoll.SocketsQueue;

			// make available to next one
			socketQueue.Enqueue(socket);

			// this may defeat the purpose of a poll (if it puts the SO socket in a time_wait state)
			// socket.Disconnect(endpoint);
		}

		public void Dispose()
		{
			InternalDispose(true);
		}

		private void Untrack(IZmqSocket socket)
		{
			lock (this._socketsInUse)
			{
				this._socketsInUse.Remove(socket);
			}
		}

		private void InvalidatePoll(string name, string reason)
		{
			if (LogAdapter.LogEnabled)
			{
				LogAdapter.LogDebug("RequestPoll", "InvalidatePoll called. " + reason);
			}

			lock (_endpoint2Sockets)
			{
				EndpointPoll endpoint;
				if (!_endpoint2Sockets.TryRemove(name, out endpoint)) return;

//				if (endpoint.monitor != null)
//				{
//					endpoint.monitor.Dispose();
//					endpoint.monitored = false;
//					endpoint.monitor = null;
//				}
				foreach (var socket in endpoint.SocketsQueue.ToArray())
				{
					socket.Dispose();
				}
			}
		}

		private void InternalDispose(bool isDisposing)
		{
			if (_disposed) return;

			this._disposed = true;

			if (isDisposing)
			{
				GC.SuppressFinalize(this);
			}

			// dispose sockets on queues
			var endpoints = this._endpoint2Sockets.Keys.ToArray();
			foreach (var endpoint in endpoints)
			{
				InvalidatePoll(endpoint, "Disposing");
			}

			IZmqSocket[] sockets;
			
			// dispose the sockets in use
			lock (this._socketsInUse)
			{
				sockets = this._socketsInUse.ToArray();
			}

			foreach (var socket in sockets)
			{
				socket.Dispose();
			}
		}
	}
}