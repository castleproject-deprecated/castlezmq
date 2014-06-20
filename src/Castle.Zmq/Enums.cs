namespace Castle.Zmq
{
	using System;

	[Flags]
	public enum RecvFlags
	{
		None = 0,
		/// <summary>
		/// Recv immediately - if the socket cant receive
		/// immediately, it will return EAGAIN, which we 
		/// translate to return null
		/// </summary>
		DoNotWait = Native.Socket.DONTWAIT
	}

	[Flags]
	public enum SendFlags
	{
		None = Wait,
		/// <summary>
		/// Waits for the socket to be in the correct state to send (that's the default behavior)
		/// </summary>
		Wait = Native.Socket.WAIT,
		/// <summary>
		/// Sends immediately - if the socket cant send immediately, it will return EAGAIN
		/// which we translate into an exception 
		/// </summary>
		DoNotWait = Native.Socket.DONTWAIT,
		/// <summary>
		/// Indicates that this is not the last frame, there's more coming. the last message
		/// should NOT have this flag
		/// </summary>
		SendMore = Native.Socket.SNDMORE
	}

	public enum SocketType
	{
		Pub = 1,
		Sub = 2,
		Req = 3,
		Rep = 4,
		Dealer = 5,
		Router = 6,
		Pull = 7,
		Push = 8,
		XPub = 9,
		XSub = 10,
		XReq = Dealer,
		XRep = Router,
	}

	public enum Transport
	{
		Tcp, Inproc
	}

	public enum SocketOpt
	{
		/// <summary> (Int32) the value needed to disable lingering on a socket's outbound queue</summary>
		NO_LINGER = 0,
		/// <summary> (UInt64) I/O thread affinity bit-mask</summary>
		AFFINITY = 4,
		/// <summary> (Byte[]) Socket identifier</summary>
		IDENTITY =  5,
		/// <summary> (Byte[]) Add subscription filter</summary>
		SUBSCRIBE =  6,
		/// <summary> (Byte[]) Remove subscription filter</summary>
		UNSUBSCRIBE =  7,
		/// <summary> (Int32) Multicast data rate in kilobits per second</summary>
		RATE =  8,
		/// <summary> (Int32) Multicast recovery period in milliseconds</summary>
		RECOVERY_IVL =  9,
		/// <summary> (Int32) Send-message buffer size in bytes</summary>
		SNDBUF = 11,
		/// <summary> (Int32) Receive-message buffer size in bytes</summary>
		RCVBUF = 12,
		/// <summary> (Int32) 1 if more message frames are available, 0 otherwise</summary>
		RCVMORE = 13,
		/// <summary> (IntPtr) native file descriptor</summary>
		FD = 14,
		/// <summary> (Int32) Socket event state, see all: Polling</summary>
		EVENTS = 15,
		/// <summary> (Int32) Socket type</summary>
		TYPE = 16,
		/// <summary> (Int32) Pause before shutdown in milliseconds</summary>
		LINGER = 17,
		/// <summary> (Int32) Pause before reconnect in milliseconds</summary>
		RECONNECT_IVL = 18,
		/// <summary> (Int32) Maximum number of queued peers</summary>
		BACKLOG = 19,
		/// <summary> (Int32) Maximum reconnection interval in milliseconds</summary>
		RECONNECT_IVL_MAX = 21,
		/// <summary> (Int64) Maximum inbound message size in bytes</summary>
		MAXMSGSIZE = 22,
		/// <summary> (Int32) Maximum number of outbound queued messages</summary>
		SNDHWM = 23,
		/// <summary> (Int32) Maximum number of inbound queued messages</summary>
		RCVHWM = 24,
		/// <summary> (Int32) Time-to-live for each multicast packet in network-hops</summary>
		MULTICAST_HOPS = 25,
		/// <summary> (Int32) Timeout period for inbound messages in milliseconds</summary>
		RCVTIMEO = 27,
		/// <summary> (Int32) Timeout period for outbound messages in milliseconds</summary>
		SNDTIMEO = 28,
		/// <summary> (String) Last address bound to endpoint</summary>
		LAST_ENDPOINT = 32,
		/// <summary> (Int32) 1 to error on unroutable messages, 0 to silently ignore</summary>
		ROUTER_MANDATORY = 33,
		/// <summary> (Int32) Override OS-level TCP keep-alive</summary>
		TCP_KEEPALIVE = 34,
		/// <summary> (Int32) Override OS-level TCP keep-alive</summary>
		TCP_KEEPALIVE_CNT = 35,
		/// <summary> (Int32) Override OS-level TCP keep-alive</summary>
		TCP_KEEPALIVE_IDLE = 36,
		/// <summary> (Int32) Override OS-level TCP keep-alive</summary>
		TCP_KEEPALIVE_INTVL = 37,
		/// <summary> (Byte[]) TCP/IP filters</summary>
		TCP_ACCEPT_FILTER = 38,
		/// <summary> (Int32) 1 to limit queuing to only completed connections, 0 otherwise</summary>
		IMMEDIATE = 39,
		/// <summary> (Int32) 1 will resend duplicate messages</summary>
		XPUB_VERBOSE = 40,
		/// <summary> (Int32) 1 to enable IPv6 on the socket, 0 to restrict to only IPv4</summary>
		IPV6 = 42,
		/// <summary> (Int32) 1 to make socket act as server for PLAIN security, 0 otherwise</summary>
		PLAIN_SERVER = 44,
		/// <summary> (String) Sets the user name for outgoing connections over TCP or IPC</summary>
		PLAIN_USERNAME = 45,
		/// <summary> (String) Sets the password for outgoing connections over TCP or IPC</summary>
		PLAIN_PASSWORD = 46,
		/// <summary> (Int32) 1 to make socket act as server for CURVE security, 0 otherwise</summary>
		CURVE_SERVER = 47,
		/// <summary> (String or Byte[]) sets the long-term public key on a client or server socket</summary>
		CURVE_PUBLICKEY = 48,
		/// <summary> (String or Byte[]) sets the long-term secret key on a client socket</summary>
		CURVE_SECRETKEY = 49,
		/// <summary> (String or Byte[]) sets the long-term server key on a client socket</summary>
		CURVE_SERVERKEY = 50,
		/// <summary> (Int32) 1 to automatically send an empty message on new connection, 0 otherwise</summary>
		PROBE_ROUTER = 51,
		/// <summary> (Int32) 1 to prefix messages with explicit request ID, 0 otherwise</summary>
		REQ_CORRELATE = 52,
		/// <summary> (Int32) 1 to relax strict alternation between ZMQ.REQ and ZMQ.REP, 0 otherwise</summary>
		REQ_RELAXED = 53,
		/// <summary> (Int32) 1 to keep last message in queue (ignores high-water mark options), 0 otherwise</summary>
		CONFLATE = 54,
		/// <summary> (String) Sets authentication domain</summary>
		ZAP_DOMAIN = 55, 
	}
}