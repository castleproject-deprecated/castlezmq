namespace Castle.Facilities.Zmq.Rpc.Internal
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Encode the parameter types for efficient transport
	/// </summary>
	internal static class TypeShortnameLookup
	{
		private static readonly Dictionary<string, Type> _short2Type = new Dictionary<string, Type>(StringComparer.Ordinal);
		private static readonly Dictionary<Type, string> _type2short = new Dictionary<Type, string>();

		static TypeShortnameLookup()
		{
			_short2Type["str"] = typeof (string);
			_short2Type["int16"] = typeof (Int16);
			_short2Type["int32"] = typeof (Int32);
			_short2Type["int64"] = typeof (Int64);
			_short2Type["uint16"] = typeof (UInt16);
			_short2Type["uint32"] = typeof (UInt32);
			_short2Type["uint64"] = typeof (UInt64);
			_short2Type["single"] = typeof (Single);
			_short2Type["double"] = typeof (Double);
			_short2Type["decimal"] = typeof (Decimal);
			_short2Type["datetime"] = typeof (DateTime);
			_short2Type["guid"] = typeof (Guid);

			// reflect entries on the other lookup table
			foreach (KeyValuePair<string, Type> pair in _short2Type)
			{
				_type2short[pair.Value] = pair.Key;
			}
		}

		public static string GetName(Type type)
		{
			string name;
			if (_type2short.TryGetValue(type, out name))
			{
				return name;
			}
			var shortname = type.AssemblyQualifiedName;
			var index1 = shortname.IndexOf(',');
			var index2 = shortname.IndexOf(',', index1 + 1);
			shortname = shortname.Substring(0, index2);
			return shortname;
		}

		public static Type GetType(string name)
		{
			Type type;
			if (_short2Type.TryGetValue(name, out type))
			{
				return type;
			}
			return Type.GetType(name, true);
		}
	}
}