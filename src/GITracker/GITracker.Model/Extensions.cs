using System;
using System.Reflection;

namespace GITracker.Model
{
	public static class MiscExtensions
	{
		public static bool IsNullOrDefault<T>(this T? value) where T : struct
		{
			return value == null || value.Value.Equals(default(T));
		}

		public static bool IsNullOrDefault(this object value)
		{
			if (value == null) return true;

			var defaultValue = value.GetType().Default();
			return value.Equals(defaultValue);
		}

		public static object Default(this Type type)
		{
			if (type == null || !type.GetTypeInfo().IsValueType || type == typeof(void))
				return null;

			if (type.GetTypeInfo().ContainsGenericParameters)
			{
				throw new ArgumentException($"Cannot determine default value for {type} with generic parameters.");
			}

			// If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct/enum), return a 
			//  default instance of the value type
			if (type.GetTypeInfo().IsPrimitive || !type.GetTypeInfo().IsNotPublic)
			{
				try
				{
					return Activator.CreateInstance(type);
				}
				catch (Exception ex)
				{
					throw new ArgumentException($"Cannot create default instance of {type}. {ex.Message}", ex);
				}
			}

			throw new ArgumentException($"{type} is not a publicly-visible type, so the default value cannot be retrieved.");
		}
	}
}
