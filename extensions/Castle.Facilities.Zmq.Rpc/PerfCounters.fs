module PerfCounters

    open Castle.Zmq.Counters

    let sentCounter = PerfCounterRegistry.Get(PerfCounters.NumberOfRequestsSent)
    let receivedCounter = PerfCounterRegistry.Get(PerfCounters.NumberOfResponseReceived)

    let private isPerfCountingEnable = ref false

    let isPerfCounterEnabled = !isPerfCountingEnable

    let setIsEnabled (v:bool) =
        isPerfCountingEnable := v


    let inline IncrementSent () = 
        if isPerfCounterEnabled 
        then sentCounter.Increment() |> ignore

    let inline IncrementRcv () = 
        if isPerfCounterEnabled 
        then receivedCounter.Increment() |> ignore

