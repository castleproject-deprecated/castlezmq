namespace Castle.Facilities.Zmq.Rpc.Internal
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;

	internal static class ReflectionUtils
	{
		/// <summary>
		/// true if type is array, IEnumerable, etc
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsCollectionType(Type type)
		{
			if (type.IsArray) return true;
			if (type == typeof(IList)) return true; 
			if (type == typeof(string)) return false;

			if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof (IEnumerable<>)) return true;

				var types = type.FindInterfaces((typ, cri) => (
					typ.IsGenericType && typ.GetGenericTypeDefinition() == typeof(IEnumerable<>)), null);
				return types.Length != 0;
			}

			return false;
		}

		private static readonly object[] EmptyArray = new object[0];

		public static object[] NormalizeToArray(object collection, Type collType)
		{
			if (collection == null) return EmptyArray;

			if (collType.IsArray)
			{
				return (object[]) collection;
			}

			if (collType.IsGenericType && collType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				var items = (IEnumerable) collection;
				var newArray = new ArrayList();
				foreach (var item in items) newArray.Add(item);
				
				return newArray.ToArray();
			}

			throw new ArgumentException("Unsupported collection type " + collType);
		}

		public static Array MakeStronglyTypedArray()
		{
			throw new NotImplementedException();
		}

		public static IEnumerable MakeStronglyTypedEnumerable()
		{
			throw new NotImplementedException();
		}
	}
}