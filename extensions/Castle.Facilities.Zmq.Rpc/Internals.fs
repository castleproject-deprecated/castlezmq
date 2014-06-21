namespace Castle.Facilities.Zmq.Rpc.Internals

    open System
    open System.IO
    open System.Diagnostics
    open System.Reflection
    open System.Collections.Generic
    open System.Collections.Concurrent
    open System.Threading
    open Castle.Zmq
    open Castle.Core
    open Castle.Core.Configuration
    open Castle.Core.Interceptor
    open Castle.DynamicProxy
    open Castle.Windsor
    open Castle.MicroKernel
    open Castle.MicroKernel.Facilities
    open Castle.MicroKernel.ModelBuilder.Inspectors
    open Castle.MicroKernel.Registration
    open System.Runtime.Remoting.Messaging
    open Castle.Zmq.Rpc.Model
    open Castle.Zmq.Extensions


    type Dispatcher(kernel:IKernel) =
        // static let logger = log4net.LogManager.GetLogger(typeof<Dispatcher>)
        
        member this.Invoke(target:string, methd:string, parms: ParamTuple array, meta: string array) = 

            let targetType = resolvedType(target)

            let instance = kernel.Resolve(targetType)

            let methodBase : MethodInfo = 
                if meta <> null 
                then
                    let methodMeta = deserialize_method_meta meta  
                    targetType.GetMethod(methd, BindingFlags.Instance ||| BindingFlags.Public, null, methodMeta, null)
                else targetType.GetMethod(methd, BindingFlags.Instance ||| BindingFlags.Public)

            let methodMeta = 
                methodBase.GetParameters() 
                |> Array.map (fun p -> p.ParameterType)

            let args = deserialize_params parms methodMeta
            // let args = deserialize_params parms (methodBase.GetParameters() |> Array.map (fun p -> p.ParameterType))

            let result = methodBase.Invoke(instance, args)
            (result, methodBase.ReturnType)


    type RemoteRequestListener(bindAddress:String, workers:int, zContextAccessor:IZmqContext, dispatcher:Dispatcher) =
        static let logger = log4net.LogManager.GetLogger(typeof<RemoteRequestListener>)

        let mutable disposed = false
        let mutable pool:WorkerPool = null

        let config = lazy
                        let parts = bindAddress.Split(':')
                        // ZConfig(parts.[0], Convert.ToUInt32(parts.[1]), Transport.TCP)
                        ()

        member this.thread_worker (socket:IZmqSocket) = 

            // use socket = zContextAccessor.Rep()
            try
                // socket.SetRecvTimeout(1000)
                // socket.Connect(config.Force().Local)

                while (not disposed) do
                    let buffer = socket.Recv()
                    if (buffer <> null) then
                        let reply = this.GetReplyFor(buffer);
                        let reply = 
                            if reply <> null then reply 
                            else Array.zeroCreate<byte> 0

                        socket.Send(  reply );
                    ()
            with
                | ex -> logger.Fatal("Error in worker thread", ex)

        member this.GetReplyFor(message) =             
            let response = 
                try
                    let request = deserialize_with_protobuf<RequestMessage>(message);

                    try
                        let result = 
                            dispatcher.Invoke(request.TargetService, request.TargetMethod, request.Params, request.ParamTypes)
                        
                        build_response (fst result) (snd result)
                    with
                        | :? TargetInvocationException as ex ->
                            let e = ex.InnerException 
                            logger.Error("Error executing remote invocation " + request.TargetService + "." + request.TargetMethod, e)
                            build_response_with_exception (e.GetType().Name) e.Message
                        | ex -> 
                            logger.Error("Error executing remote invocation " + request.TargetService + "." + request.TargetMethod, ex)
                            build_response_with_exception (ex.GetType().Name) ex.Message
                with
                    | ex -> 
                        logger.Error("Error executing remote invocation", ex)
                        build_response_with_exception (ex.GetType().Name) ex.Message

            try
                let buffer = serialize_with_protobuf(response)
                buffer
            with
                | ex -> 
                    serialize_with_protobuf ( ResponseMessage(null, null, ExceptionInfo(ex.GetType().Name, ex.Message)) )

        interface IStartable with
            override this.Start() = 
                logger.Debug("Starting " + this.GetType().Name)

                let c = config.Force()

                if (pool <> null) then pool.Dispose()

                pool <- new WorkerPool(zContextAccessor, c.ToString(), c.Local, new ThreadStart(this.thread_worker), workers)
                pool.Start()

                logger.InfoFormat("Binding {0} on {1}:{2} with {3} workers", this.GetType().Name, c.Ip, c.Port, workers)

            override this.Stop() = 
                if pool <> null then
                    pool.Stop()

        interface IDisposable with
            override this.Dispose() =
                disposed <- true
                if pool <> null then
                    pool.Dispose()


    type RemoteRequest(zContextAccessor:Castle.Zmq.Context, message:RequestMessage, endpoint:string) = 
        inherit BaseRequest<ResponseMessage>(zContextAccessor)

        let config = lazy
                        let parts = endpoint.Split(':')
                        ZConfig(parts.[0], Convert.ToUInt32(parts.[1]), Transport.TCP)

        override this.GetConfig() = config.Force()

        override this.Timeout with get() = 30 * 1000

        override this.InternalGet(socket) =
            socket.Send(serialize_with_protobuf(message))
    
            PerfCounters.IncrementSent ()

            let bytes = socket.Recv()
            
            if bytes <> null then
                PerfCounters.IncrementRcv ()
//                elapsedCounter.IncrementBy(watch.ElapsedTicks) |> ignore
//                baseElapsedCounter.Increment() |> ignore

            if bytes = null then
                let m = "Remote call took too long to respond. Is the server up? " + (config.Value.ToString())
                ResponseMessage(null, null, ExceptionInfo("Timeout", m))
            else
                deserialize_with_protobuf<ResponseMessage>(bytes)


    type RemoteRouter() =
        let routes = Dictionary<string, string>()

        member this.ParseRoutes(config:IConfiguration) =
            for child in config.Children do
                routes.Add(child.Attributes.["assembly"], child.Attributes.["address"])

        member this.GetEndpoint(assembly:Assembly) =
            let overriden = CallContext.GetData("0mq.facility.endpoint") :?> string

            if String.IsNullOrEmpty(overriden) then routes.[assembly.GetName().Name] else overriden

        member this.ReRoute(assembly: string, address: string) =
            routes.[assembly] <- address


    type RemoteRequestInterceptor(zContextAccessor:ZContextAccessor, router:RemoteRouter) =
        static let logger = log4net.LogManager.GetLogger(typeof<RemoteRequestInterceptor>)

        interface IInterceptor with
            
            member this.Intercept(invocation) =
                let stopwatch = System.Diagnostics.Stopwatch()
                
                if logger.IsDebugEnabled then
                    stopwatch.Start()

                try
                    if invocation.TargetType <> null then
                        invocation.Proceed()
                    else
                        let pInfo = invocation.Method.GetParameters()
                        let pTypes = pInfo |> Array.map (fun p -> p.ParameterType)
                        let args = 
                            serialize_parameters (invocation.Arguments) pTypes 

                        let methodMeta = serialize_method_meta pInfo

                        let request = RequestMessage(invocation.Method.DeclaringType.AssemblyQualifiedName, 
                                                     invocation.Method.Name, args, methodMeta)
                        let endpoint = router.GetEndpoint(invocation.Method.DeclaringType.Assembly)

                        let request = RemoteRequest(zContextAccessor, request, endpoint)
                        let response = request.Get()

                        if response.ExceptionInfo <> null then
                            let msg = "Remote server threw " + (response.ExceptionInfo.Typename) + " with message " + (response.ExceptionInfo.Message)
                            raise (new Exception(msg))

                        else if invocation.Method.ReturnType <> typeof<Void> then
                            invocation.ReturnValue <- deserialize_reponse response invocation.Method.ReturnType
                                    
                finally
                    if logger.IsDebugEnabled then
                        logger.Debug("Intercept took " + (stopwatch.ElapsedMilliseconds.ToString()))
                    
        interface IOnBehalfAware with
            member this.SetInterceptedComponentModel(target) = ()

    type RemoteRequestInspector() =
        inherit MethodMetaInspector()

        override this.ObtainNodeName() = "remote-interceptor"

        member this.add_interceptor(model:ComponentModel) =
            model.Dependencies.Add(new DependencyModel(this.ObtainNodeName(), typeof<RemoteRequestInterceptor>, false))
            model.Interceptors.Add(new InterceptorReference(typeof<RemoteRequestInterceptor>))
        
        override this.ProcessModel(kernel, model) =
            if (model.Services |> Seq.exists (fun s -> s.IsDefined(typeof<RemoteServiceAttribute>, false))) then
                this.add_interceptor(model)

    [<AllowNullLiteralAttribute>]
    type Reaper(zContextAccessor:ZContextAccessor) =
        static let logger = log4net.LogManager.GetLogger(typeof<Reaper>)

        let mutable _disposed = false

        let dispose() = 
            if (_disposed = false) then
                _disposed <- true
                try
                    logger.Info("Disposing ZeroMQ Facility...")

                    // listeners: RemoteRequestListener[]
                    // listeners |> Seq.iter (fun listener -> listener.Dispose())

                    zContextAccessor.Dispose()

                    logger.Info("Disposed ZeroMQ Facility.")
                 with
                    | ex -> logger.Error("Error disponsing ZeroMQ Facility components", ex)

        interface IDisposable with
            member this.Dispose() = dispose()



            (*
    type AlternativeRouteContext(route: string) =
        do
           CallContext.SetData("0mq.facility.endpoint", route)

        interface IDisposable with
            member x.Dispose() =
                CallContext.SetData("0mq.facility.endpoint", null)

        static member For(r: string) =
            (new AlternativeRouteContext(r)) :> IDisposable
            *)
