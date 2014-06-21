namespace Castle.Facilities.Zmq.Rpc.Internals

    open System
    open System.Collections.Generic
    open System.Reflection
    open System.IO
    open ProtoBuf
    open System.Runtime.Serialization
    open Castle.Zmq.Rpc.Model


    [<AutoOpen>]
    module TransportSerialization =

        let inline slice_buffer (buffer:byte[]) (size:int64) = 
            let small = int(size)
            if buffer.Length = small then buffer
            else
                let smallbuffer = Array.zeroCreate<byte> (small) 
                Buffer.BlockCopy(buffer, 0, smallbuffer, 0, small)
                smallbuffer
            
        let deserialize_array (arType:Type) (buffer:byte[]) = 
            let stream = new MemoryStream(buffer)
            // ProtoBuf.Meta.RuntimeTypeModel.Default.DeserializeItems(stream, PrefixStyle. arr)

            let res = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(stream, null, arType)
            res :?> System.Collections.IList

        // return tuple (buffer + typename) array
        let serialize_array arr = 
            let stream = new MemoryStream()
            ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, arr)
            slice_buffer (stream.GetBuffer()) stream.Length




        let serialize_param (pType:Type) (v:obj) = 
            // let pType = t.ParameterType
            if pType.IsArray then failwithf "ZMQ facility doesnt support array as parameters"

            if v = null then null 
            else 
                let str = 
                    if pType.IsPrimitive then
                        v.ToString() 
                    elif pType = typeof<string> then
                        v.ToString()
                    // non primitive but common
                    elif pType = typeof<decimal> then
                        v.ToString()
                    // Structs
                    elif pType = typeof<Guid> then
                        v.ToString() 
                    elif pType = typeof<DateTime> then
                        let dt = (v :?> DateTime).Ticks
                        dt.ToString() 
                    // Enum
                    elif pType.IsEnum then
                        System.Convert.ToInt32( v ).ToString()
                    else null
            
                let valToSerialize = 
                    if str <> null then str :> obj, "string"
                    else v, (v.GetType().AssemblyQualifiedName)

                let value = fst valToSerialize
                let typename = snd valToSerialize
                    
                use stream = new MemoryStream()
                ProtoBuf.Meta.RuntimeTypeModel.Default.Serialize(stream, value)
                let buffer = slice_buffer (stream.GetBuffer()) stream.Length
                ParamTuple(buffer, typename)
                

        let serialize_parameters (originalArgs:obj[]) (ps:Type[]) = 
            let args = 
                ps
                |> Seq.mapi (fun i t -> serialize_param t (originalArgs.[i]))
                |> Seq.toArray
            args


        let deserialize_param (param:ParamTuple) (expectedParamType:Type) = 
            let serializedType = 
                if param.TypeName = null then raise (ArgumentException("ParamTuple has TypeName null - check if you're using compatible versions of the facility"))
                    
                if param.TypeName = "string" 
                then typeof<string>
                else Type.GetType(param.TypeName, true)

            let buffer = param.SerializedValue
            let res = ProtoBuf.Meta.RuntimeTypeModel.Default.Deserialize(new MemoryStream(buffer), null, serializedType)
            
            if serializedType = expectedParamType then
                res
            else 
                let v : obj = res 

                let pType = expectedParamType

                if pType = typeof<decimal> then
                    System.Convert.ToDecimal(v) :> obj
                                        
                elif pType.IsEnum then
                    let iVal = System.Convert.ToInt32(v)
                    iVal :> obj

                elif pType = typeof<int> then
                    System.Convert.ToInt32(v) :> obj
                elif pType = typeof<int16> then
                    System.Convert.ToInt16(v) :> obj
                elif pType = typeof<int64> then
                    System.Convert.ToInt64(v) :> obj
                elif pType = typeof<byte> then
                    System.Convert.ToByte(v) :> obj

                elif pType = typeof<float32> then
                    System.Convert.ToSingle(v) :> obj
                                        
                elif pType = typeof<double> then
                    System.Convert.ToDouble(v) :> obj

                elif pType = typeof<Guid> then
                    Guid.Parse(v.ToString()) :> obj

                elif pType = typeof<DateTime> then
                    let long = Convert.ToInt64(v)
                    DateTime(long) :> obj
                else
                    v


        let deserialize_params (parms:ParamTuple array) (ps:Type[]) = 
            // ref / out params not supported
            if parms = null then null
            else 
                parms 
                |> Seq.mapi (fun i v -> deserialize_param v (ps.[i])) 
                |> Seq.toArray


        let serialize_with_protobuf(instance: 'a) =
            // let watch = System.Diagnostics.Stopwatch()
            // watch.Start()
            use input = new MemoryStream()
            Serializer.Serialize(input, instance)
            input.Flush()
            // watch.Stop()
            // Console.Out.WriteLine ("serialize_with_protobuf {0} elapsed {1}", input.Length, watch.ElapsedTicks)
            input.ToArray()

        let deserialize_with_protobuf<'a> (bytes:byte array) : 'a =
            use input = new MemoryStream(bytes)
            Serializer.Deserialize<'a>(input)

        let serialize_method_meta (ps:ParameterInfo[]) = 
            ps |> Array.map (fun p -> p.ParameterType) |> Array.map (fun t -> t.AssemblyQualifiedName)

        let _typename2Type = System.Collections.Concurrent.ConcurrentDictionary<string,Type>(StringComparer.Ordinal)

        let resolvedType (target: string) = 
            let res, t = _typename2Type.TryGetValue target
            if not res then
                let tgtType = Type.GetType(target)
                _typename2Type.TryAdd (target, tgtType) |> ignore
                tgtType
            else t

        let deserialize_method_meta (meta: string array) = 
            if (meta = null || meta.Length = 0)
            then Array.empty<Type>
            else meta |> Array.map (fun m -> resolvedType(m))


        let build_response (result:obj) (retType) = 
            if retType = typeof<Void> 
            then ResponseMessage()
            else 
                if is_collection_type (retType) then
                    let arrayRes = to_array result
                    let sArray = serialize_array arrayRes
                    ResponseMessage(sArray, retType.AssemblyQualifiedName, null) (* , ReturnValueArrayType = types) *)
                elif result <> null then
                    let retTuple = serialize_param (result.GetType()) result
                    ResponseMessage(retTuple.SerializedValue, retTuple.TypeName, null)
                else 
                    ResponseMessage()

        let build_response_with_exception (typename) (msg) = 
            ResponseMessage(null, null, ExceptionInfo(typename, msg))
        

        let deserialize_reponse (response:ResponseMessage) (retType:Type) = 
            
            if is_collection_type (retType) then
                if retType.IsArray then
                    let arrayElemType = retType.GetElementType()
                    let items = deserialize_array retType response.ReturnValue
                    (make_strongly_typed_array arrayElemType (items)) :> obj
                else
                    let itemType = retType.GetGenericArguments().[0]
                    let items = deserialize_array retType response.ReturnValue
                    (make_strongly_typed_enumerable itemType (items))
            else
                let serializedType = response.ReturnValueType
                let serializedBuffer = response.ReturnValue

                if serializedType = null && serializedBuffer = null 
                then null
                else deserialize_param (ParamTuple(serializedBuffer, serializedType)) retType
        
        
            