namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;

	/// <summary>
	/// Convinience API exposed for sockets. 
	/// </summary>
	public static class SocketExtensions
	{
		/// <summary>
		/// Returns true if the last message received has indicated 
		/// that there's more to come (part of a multipart message). 
		/// Otherwise false.
		/// </summary>
		/// <param name="source"></param>
		/// <returns>true if last message was a multipart message</returns>
		public static bool HasMoreToRecv(this IZmqSocket source)
		{
			return source.GetOption<bool>(SocketOpt.RCVMORE);
		}

		public static byte[] Recv(this IZmqSocket source, RecvFlags flags)
		{
			return source.Recv((int) flags);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="encoding">If not specified, defaults to UTF8</param>
		/// <returns></returns>
		public static string RecvString(this IZmqSocket source, Encoding encoding = null)
		{
			return RecvString(source, RecvFlags.None, encoding);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="flags"></param>
		/// <param name="encoding">If not specified, defaults to UTF8</param>
		/// <returns></returns>
		public static string RecvString(this IZmqSocket source, RecvFlags flags, Encoding encoding = null)
		{
			var buffer = Recv(source, flags);
			if (buffer == null) return null;

			encoding = encoding ?? Encoding.UTF8;

			return encoding.GetString(buffer);
		}

		/// <summary>
		/// Sends the byte[] message and uses the specified flags to configure the sending behavior.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="message">The byte array to send</param>
		/// <param name="flags">Send flags configuring the sending operation</param>
		public static void Send(this IZmqSocket source, byte[] message, SendFlags flags)
		{
			bool hasMore = (flags & SendFlags.SendMore) != 0;
			bool doNotWait = (flags & SendFlags.DoNotWait) != 0;

			source.Send(message, hasMore, doNotWait);
		}

		/// <summary>
		/// Sends a string message, converting to bytes using the 
		/// encoding specified. If none, uses UTF8.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="message">The string message</param>
		/// <param name="encoding">If not specified, defaults to UTF8</param>
		/// <param name="hasMoreToSend">Flag indicating whether it's a multipart message</param>
		/// <param name="noWait">Indicates that the sock must send the message immediately</param>
		public static void Send(this IZmqSocket source, string message, Encoding encoding = null, 
								bool hasMoreToSend = false, bool noWait = false)
		{
			if (message == null) throw new ArgumentNullException("message");

			encoding = encoding ?? Encoding.UTF8;
			var buffer = encoding.GetBytes(message);

			source.Send(buffer, hasMoreToSend, noWait);
		}

		public static T GetOption<T>(this IZmqSocket source, SocketOpt option)
		{
			return source.GetOption<T>((int) option);
		}

		public static void SetOption(this IZmqSocket source, SocketOpt option, bool value)
		{
			SetOption(source, option, value ? 1 : 0);
		}

		public static void SetOption(this IZmqSocket source, SocketOpt option, int value)
		{
			source.SetOption<int>((int)option, value);
		}

		public static void SetOption(this IZmqSocket source, SocketOpt option, Int64 value)
		{
			source.SetOption<Int64>((int)option, value);
		}

		public static void SetOption(this IZmqSocket source, SocketOpt option, string value)
		{
			if (value == null) throw new ArgumentNullException("value");

			source.SetOption<string>((int)option, value);
		}

		public static void SetOption(this IZmqSocket source, SocketOpt option, byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			source.SetOption<byte[]>((int)option, buffer);
		}

		/// <summary>
		/// Subscribe to everything a publisher sends. 
		/// Internally sends an empty byte array
		/// </summary>
		public static void SubscribeAll(this IZmqSocket source)
		{
			source.Subscribe("");
		}

		/// <summary>
		/// Unsubscribe to everything a publisher sends.
		/// Internally sends an empty byte array
		/// </summary>
		public static void UnsubscribeAll(this IZmqSocket source)
		{
			source.Unsubscribe("");
		}

		/// <summary>
		/// Subscribe to all topics specified.
		/// </summary>
		public static void Subscribe(this IZmqSocket source, string[] topics)
		{
			foreach (var topic in topics)
			{
				source.Subscribe(topic);
			}
		}

		/// <summary>
		/// Unsubscribe from all topics specified.
		/// </summary>
		public static void Unsubscribe(this IZmqSocket source, string[] topics)
		{
			foreach (var topic in topics)
			{
				source.Unsubscribe(topic);
			}
		}
	}
}