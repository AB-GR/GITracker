using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace GITracker.Model
{
	public class ModelCache : ICache
	{
		private readonly ConcurrentDictionary<object, Tuple<Type[], object>> cache = new ConcurrentDictionary<object, Tuple<Type[], object>>();

		public bool CachingEnabled => true;

		public T GetOrAdd<T>(object key, Func<T> loader, params Type[] dependencies)
		{
			if (dependencies.Length > 0 && dependencies[0] == null)
				throw new InvalidOperationException("You've likely called a wrong overload!");

			return GetOrAdd(key, loader, null, dependencies);
		}

		public T GetOrAdd<T>(object key, Func<T> loader, Func<T, bool> shouldCache, params Type[] dependencies)
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}

			if (loader == null)
			{
				throw new ArgumentNullException("loader");
			}

			if (!this.CachingEnabled)
			{
				return loader();
			}

			if (!cache.ContainsKey(key))
			{
				var allDependencies = GetBaseTypes<T>();
				if (dependencies != null)
					allDependencies.AddRange(dependencies);

				var result = loader();
				if (shouldCache != null && !shouldCache(result))
					return result;

				cache.TryAdd(key, Tuple.Create(allDependencies.ToArray(), (object)result));
			}

			return (T)cache[key].Item2;
		}

		private List<Type> GetBaseTypes<T>()
		{
			var type = typeof(T);
			return type.GetTypeInfo().IsGenericType ? type.GenericTypeArguments.ToList() : new List<Type> { type };
		}

		public void Expire(object key)
		{
			Tuple<Type[], object> returnValue;

			if (cache.ContainsKey(key))
			{
				cache.TryRemove(key, out returnValue);
			}
		}

		public void Modified(Type t)
		{
			Tuple<Type[], object> returnValue;

			foreach (var ci in cache.ToArray())
			{
				if (ci.Value.Item1.Contains(t))
				{
					cache.TryRemove(ci.Key, out returnValue);
				}
			}
		}
	}
}
