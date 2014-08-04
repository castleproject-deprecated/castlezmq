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
		private readonly IZmqContext _context;
		private readonly ConcurrentDictionary<string, ConcurrentQueue<IZmqSocket>> _endpoint2Sockets; 
		private readonly HashSet<IZmqSocket> _socketsInUse; 
		private volatile bool _disposed;

		public RequestPoll(IZmqContext context)
		{
			this._context = context;
			this._endpoint2Sockets = new ConcurrentDictionary<string, ConcurrentQueue<IZmqSocket>>(StringComparer.InvariantCultureIgnoreCase);
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

			var socketQueue = _endpoint2Sockets.GetOrAdd(endpoint, _ => new ConcurrentQueue<IZmqSocket>());

			IZmqSocket socket;
			if (!socketQueue.TryDequeue(out socket))
			{
				socket = this._context.CreateSocket(SocketType.Req);
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

		public void Return(IZmqSocket socket, string endpoint)
		{
			if (socket == null) throw new ArgumentNullException("socket");
			if (string.IsNullOrEmpty(endpoint)) throw new ArgumentNullException("endpoint");

			if (_disposed)
			{
				socket.Dispose();
				return;
			}

			// untrack
			lock (this._socketsInUse)
				this._socketsInUse.Remove(socket);

			var socketQueue = _endpoint2Sockets.GetOrAdd(endpoint, _ => new ConcurrentQueue<IZmqSocket>());

			// make available to next one
			socketQueue.Enqueue(socket);

			// this may defeat the purpose of a poll (if it puts the SO socket in a time_wait state)
			// socket.Disconnect(endpoint);
		}

		public void Dispose()
		{
			InternalDispose(true);
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
			var endpoints = this._endpoint2Sockets.ToArray();
			foreach (var endpoint in endpoints)
			foreach (var socket in endpoint.Value.ToArray())
			{
				socket.Dispose();
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