namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;

	public static class SocketExtensions
	{
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

		public static void Send(this IZmqSocket source, byte[] message, SendFlags flags)
		{
			bool hasMore = (flags & SendFlags.SendMore) != 0;
			bool doNotWait = (flags & SendFlags.DoNotWait) != 0;

			source.Send(message, hasMore, doNotWait);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="message"></param>
		/// <param name="encoding">If not specified, defaults to UTF8</param>
		public static void Send(this IZmqSocket source, string message, Encoding encoding = null)
		{
			if (message == null) throw new ArgumentNullException("message");

			encoding = encoding ?? Encoding.UTF8;
			var buffer = encoding.GetBytes(message);

			source.Send(buffer);
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
			const int len = sizeof(int);

			MarshalExt.AllocAndRun((valPtr) =>
			{
				Marshal.WriteInt32(valPtr, value);
				source.SetOption((int)option, valPtr, sizeof(int));
			}, len);
		}

		public static void SetOption(this IZmqSocket source, SocketOpt option, Int64 value)
		{
			const int len = sizeof (Int64);

			MarshalExt.AllocAndRun((valPtr) =>
			{
				Marshal.WriteInt64(valPtr, value);
				source.SetOption((int)option, valPtr, len);
			}, len);
		}

		public static void SetOption(this IZmqSocket source, SocketOpt option, string value)
		{
			if (value == null) throw new ArgumentNullException("value");

			var buffer = Encoding.UTF8.GetBytes(value);

			SetOption(source, option, buffer);
		}

		public static void SetOption(this IZmqSocket source, SocketOpt option, byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");

			var len = buffer.Length;

			MarshalExt.AllocAndRun((valPtr) =>
			{
				Marshal.Copy(buffer, 0, valPtr, len);
				source.SetOption((int)option, valPtr, len);
			}, len);
		}

		public static void SubscribeAll(this IZmqSocket source, string[] topics)
		{
			throw new NotImplementedException();
		}

		public static void UnsubscribeAll(this IZmqSocket source, string[] topics)
		{
			throw new NotImplementedException();
		}
	}
}