namespace Castle.Facilities.Zmq.Rpc

    open System

    [<AttributeUsage(AttributeTargets.Interface)>]
    type RemoteServiceAttribute() =
        inherit Attribute()