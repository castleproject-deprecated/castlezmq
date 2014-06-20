namespace Castle.Zmq.Extensions
{
	using System;
	using System.Threading;

	public class WorkerPool : SharedQueue
	{
		private readonly Context _ctx;
		private readonly string _backendEndpoint;
		private readonly Action<IZmqSocket> _proc;
		private readonly int _workers;
		private volatile bool _running;

		public WorkerPool(Context ctx, string frontEndEndpoint, string backendEndpoint, Action<IZmqSocket> proc, int workers)
			: base(ctx, frontEndEndpoint, backendEndpoint)
		{
			_ctx = ctx;
			_backendEndpoint = backendEndpoint;
			_proc = proc;
			_workers = workers;
		}

		public override void Start()
		{
			this._running = true;

			for (int i = 0; i < _workers; i++)
			{
				ThreadPool.UnsafeQueueUserWorkItem(WorkerProc, null);
			}

			base.Start();
		}

		private void WorkerProc(object state)
		{
			using (var socket = this._ctx.CreateSocket(SocketType.Rep))
			{
				socket.Connect(_backendEndpoint);

				var polling = new Polling(PollingEvents.RecvReady, socket);
				
				// once data is ready, we pass it along to the actual worker
				polling.RecvReady += _dummy => this._proc(socket);

				while (_running)
				{
					polling.PollForever(); 
				}
			}
		}

		protected override void DoDispose()
		{
			_running = false;
		}
	}
}