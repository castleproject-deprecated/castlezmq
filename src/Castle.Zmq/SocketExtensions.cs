namespace Castle.Zmq
{
	using System;
	using System.Runtime.InteropServices;
	using System.Text;

	public static class SocketExtensions
	{
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