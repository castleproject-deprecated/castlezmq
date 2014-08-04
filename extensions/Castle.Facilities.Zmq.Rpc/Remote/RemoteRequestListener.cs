namespace Castle.Facilities.Zmq.Rpc.Remote
{
	using System;
	using System.Reflection;
	using System.Threading;
	using Castle.Core;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using Castle.Facilities.Zmq.Rpc.Local;
	using Castle.Zmq;
	using Castle.Zmq.Extensions;
	using Castle.Zmq.Rpc.Model;


	/// <summary>
	/// Deserializes the <see cref="RequestMessage"/>,
	/// invokes the real method using the <see cref="LocalInvocationDispatcher"/>
	/// and returns a <see cref="ResponseMessage"/> to the requester (Zmq.Req)
	/// </summary>
	public class RemoteRequestListener : IStartable
	{
		private static int localAddUseCounter = 1;

		private readonly IZmqContext _context;
		private readonly string _endpoint;
		private readonly int _workers;
		private readonly LocalInvocationDispatcher _dispatcher;
		private readonly SerializationStrategy _serializationStrategy;

		private WorkerPool _workerPool;

		private readonly string _localEndpoint;

		public RemoteRequestListener(IZmqContext context, string endpoint, int workers, 
									 LocalInvocationDispatcher dispatcher, 
									 SerializationStrategy serializationStrategy)
		{
			this._context = context;
			this._endpoint = endpoint;
			this._workers = workers;
			this._dispatcher = dispatcher;
			this._serializationStrategy = serializationStrategy;

			var count = Interlocked.Increment(ref localAddUseCounter);
			this._localEndpoint = "inproc://rrworker_" + count;
		}

		public void Start()
		{
			this.Stop();

			this._workerPool = new WorkerPool(this._context, 
											  this._endpoint, this._localEndpoint, 
											  this.OnRequestReceived, this._workers);
			this._workerPool.Start();
		}

		public void Stop()
		{
			if (_workerPool != null)
			{
				_workerPool.Dispose();
				_workerPool = null;
			}
		}

		private void OnRequestReceived(byte[] message, IZmqSocket socket)
		{
			var reqMessage = _serializationStrategy.DeserializeRequest(message);

			ResponseMessage response = InternalDispatch(reqMessage);

			var buffer = _serializationStrategy.SerializeResponse(response);
			socket.Send(buffer);
		}

		private ResponseMessage InternalDispatch(RequestMessage reqMessage)
		{
			ResponseMessage response = null;
			try
			{
				var result =
					this._dispatcher.Dispatch(reqMessage.TargetService,
						reqMessage.TargetMethod,
						reqMessage.Params, reqMessage.ParamTypes);

				response = Builder.BuildResponse(result);
			}
			catch (TargetInvocationException ex)
			{
				var e = ex.InnerException;
				response = Builder.BuildResponse(e);
			}
			catch (Exception ex)
			{
				response = Builder.BuildResponse(ex);
			}
			return response;
		}		
	}
}
