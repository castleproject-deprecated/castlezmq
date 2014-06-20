castle.zmq
==========

Modern and robust .net binding for 0mq (zmq). From the [http://www.castleproject.org/](http://www.castleproject.org/) people.


Why
---

The existing bindings for .net are either staled or not really built for real world usage. Out of need, we had to create a new one.




Notes
-----

- The libzmq.dll included has been compiled with FD_SETSIZE=2400. The default (winsock) config is 64
- You must use the correct dll and libzmq for your processor architecture (or, for the hosting process). In other words, if the hosting process is x86, use the x86. Otherwise x64. If you don't, expect BadImageFormatException


Using it
--------

First of all, be familiar with zmq. Read its fantastic amazing guide & check their api. 

Before using it, initiate a context.

    using Castle.Zmq;
	... 
	var ctx = new Context();
	...
	// some serious profitable work here
	...
	ctx.Dispose();


**Multipart messages**

Use the `hasMoreToSend` which internally is mapped to `SNDMORE`

	socket.Send(byteBuffer1, hasMoreToSend: true);
	socket.Send(byteBuffer2, hasMoreToSend: true);
	socket.Send(finalBuffer); // no flag

On the receiving side, you can check if there's more to be received:

	var data1 = subSocket.Recv();
	if (subSocket.HasMoreToRecv()) 
	{
		var data2 = subSocket.Recv(); 
	}


**Req/Rep**

	const string MsgReq = "Hello";
	const string MsgReply = "World";

	using (var reqSocket = base.Context.CreateSocket(SocketType.Req))
	using (var repSocket = base.Context.CreateSocket(SocketType.Rep))
	{
		repSocket.Bind("tcp://0.0.0.0:90002");

		reqSocket.Connect("tcp://127.0.0.1:90002");

		reqSocket.Send(MsgReq);

		var msg = repSocket.Recv();
		var msgStr = Encoding.UTF8.GetString(msg);

		repSocket.Send(MsgReply);

		msg = reqSocket.Recv();
		msgStr = Encoding.UTF8.GetString(msg);
	}

**Pub/Sub**

A publisher should send the topic then send the data. Use the flag to indicate there's more coming:

	var pubSocket = base.Context.CreateSocket(SocketType.Pub)
	
	pubSocket.Send("topic", null, hasMoreToSend: true);
	pubSocket.Send("data");


The subscriber socket should (duh) subscribe to the topics of interest or all.

	var subSocket = base.Context.CreateSocket(SocketType.Sub)

	// specific topic
	subSocket.Subscribe("topicX");

	// everything
	subSocket.SubscribeAll();

Note that on the sub side the first Recv will get the topic, the next the actual data.


**Polling**

Use the `Polling` class to specify the events and sockets you're interested in polling. 


	using (var repSocket = base.Context.CreateSocket(SocketType.Rep))
	using (var reqSocket = base.Context.CreateSocket(SocketType.Req))
	{
		repSocket.Bind("tcp://0.0.0.0:90001");
	
		var polling = new Polling(PollingEvents.RecvReady, repSocket, reqSocket);
	
		polling.RecvReady += (socket) =>
		{
			// using socket.Recv() here is guaranted to return stuff
		};
		
		reqSocket.Connect("tcp://127.0.0.1:90001");
		reqSocket.Send("Hello");
		
		polling.PollForever(); // this returns once some socket event happens
	}


**Monitoring**

In progress

**Writing test**

The `Socket` and `Context` use the `IZmqContext` and `IZmqSocket` in order to allow you to mock/stub their real implementation.


Additions & Extensions
======================

In progress

Device
------

WorkerPool
----------

RPC
---

In progress



How to help
-----------

Send a pull request


Contact
-------

Check [Castle's Get Involved](http://www.castleproject.org/get-involved/mailing-lists/) page. 

