
namespace Castle.Facilities.Zmq.Rpc

    open System
    open Castle.Zmq
    open Castle.Facilities.Startable
    open Castle.MicroKernel.Facilities
    open Castle.MicroKernel.Registration
    open Castle.Facilities.Zmq.Rpc.Internals


    type ZmqRpcFacility() =
        inherit AbstractFacility()

        let _reaper : Ref<Reaper> = ref null
        let _isServer : Ref<bool> = ref false

        member this.setup_server() =
            let workers = if base.FacilityConfig.Attributes.["workers"] = null then 3 else Convert.ToInt32(base.FacilityConfig.Attributes.["workers"])

            let listener = Component
                            .For<RemoteRequestListener>()
                            .Parameters(
                                Parameter.ForKey("bindAddress").Eq(base.FacilityConfig.Attributes.["listen"]),
                                Parameter.ForKey("workers").Eq(workers.ToString()))

            base.Kernel.Register(listener) |> ignore

        member this.setup_client() =
            let router = base.Kernel.Resolve<RemoteRouter>()
            router.ParseRoutes(base.FacilityConfig.Children.["endpoints"])

        override this.Init() =
            if not (base.Kernel.GetFacilities() |> Seq.exists (fun f -> f.GetType() = typeof<StartableFacility>)) then
                base.Kernel.AddFacility<StartableFacility>() |> ignore

            base.Kernel.Register(Component.For<Castle.Zmq.Context>().Forward<Castle.Zmq.IZmqContext>(),
                                 Component.For<RemoteRouter>(),
                                 Component.For<Dispatcher>(),
                                 Component.For<RemoteRequestInterceptor>().LifeStyle.Transient) |> ignore

            _isServer := not (String.IsNullOrEmpty(base.FacilityConfig.Attributes.["listen"]))

            base.Kernel.ComponentModelBuilder.AddContributor(new RemoteRequestInspector())

            PerfCounters.setIsEnabled ( StringComparer.OrdinalIgnoreCase.Equals("true", base.FacilityConfig.Attributes.["usePerfCounter"]) )

            if (_isServer.Value) then
                this.setup_server()

            if base.FacilityConfig.Children.["endpoints"] <> null then
                this.setup_client()

            _reaper := base.Kernel.Register(Component.For<Reaper>()).Resolve<Reaper>() 
        
        override this.Dispose() = 
            
            if (_isServer.Value) then
                let instance = base.Kernel.Resolve<RemoteRequestListener>()
                (instance :> IDisposable).Dispose()

            if !_reaper <> null then 
                (!_reaper :> IDisposable).Dispose();

