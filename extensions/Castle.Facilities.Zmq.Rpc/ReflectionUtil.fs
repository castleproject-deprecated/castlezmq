namespace Castle.Facilities.Zmq.Rpc.Internals

    open System
    open System.Reflection
    open System.Collections
    open System.Collections.Generic

    [<AutoOpen>]
    module ReflectionUtil =

        let is_collection_type (t:Type) =
            let isGen = t.IsGenericType
            if t = typeof<string> then false
            elif isGen && (t.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>) 
            then true
            else t.IsArray

        let is_collection o =
            if o = null then false
            else
                let t = o.GetType()
                is_collection_type t

        let to_array (o:obj) =
            if o = null then [||]
            else
                let t = o.GetType()
                let isGen = t.IsGenericType
                if isGen && (t.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>) then
                    let elem = (o :?> IEnumerable<obj>) 
                    let items = 
                        seq {
                            use enumerator = elem.GetEnumerator()
                            while enumerator.MoveNext() do
                                yield enumerator.Current
                            }
                    items |> Seq.toArray
                else // isArray
                    o :?> obj[]
                    //[||]


        let make_strongly_typed_array (expectedType:Type) (items:System.Collections.IList) = 
            let typedArray = Array.CreateInstance(expectedType, items.Count)
            let len = items.Count
            for i=0 to len - 1 do
                let e = items.[i]
                typedArray.SetValue(e,i)
                ()
            typedArray

        let make_strongly_typed_enumerable (expectedType:Type) (items:System.Collections.IList) = 
            let listType = typedefof<List<_>>.MakeGenericType(expectedType)
            let typedList = Activator.CreateInstance(listType)
            let addMethod = listType.GetMethod("Add")
            let len = items.Count

            for i=0 to len - 1 do
                let e = items.[i]
                addMethod.Invoke(typedList, [| e |]) |> ignore

            typedList

