using System;
using System.Collections.Generic;
using System.Text;

namespace GITracker.Model
{
	using System;

	/// <summary>
	/// Provides methods and properties to implement caching provider.
	/// </summary>
	public interface ICache
	{
		/// <summary>
		/// Gets a value indicating whether or not caching is enabled.
		/// </summary>
		/// <value>
		///   <c>true</c> if caching is enabled; otherwise, <c>false</c>.
		/// </value>
		bool CachingEnabled { get; }

		/// <summary>
		/// Gets the item with the specified key from the cache or if the item does not exist, obtains it from the loader delegate, puts in the cache and returns it.
		/// </summary>
		/// <typeparam name="T">Item type</typeparam>
		/// <param name="key">The key.</param>
		/// <param name="loader">The loader.</param>
		/// <param name="dependencies">The default dependency is the T itself, you can define other dependencies (nested objects) part of this parameter collection.<c>true</c> cache lookup is skipped and item is immediately returned by invoking the loader.</param>
		/// <returns>The item with the specified key from the cache or the item returned by the loader.</returns>
		T GetOrAdd<T>(object key, Func<T> loader, params Type[] dependencies);

		/// <summary>
		/// Gets the item with the specified key from the cache or if the item does not exist, obtains it from the loader delegate, let you determine whether or not to cache the loaded value, if yes puts in the cache and finally returns it.
		/// </summary>
		/// <typeparam name="T">Item type</typeparam>
		/// <param name="key">The key.</param>
		/// <param name="loader">The loader.</param>
		/// <param name="shouldCache">The predicate to determine whether to cache this value (if loader is called).</param>
		/// <param name="dependencies">The default dependency is the T itself, you can define other dependencies (nested objects) part of this parameter collection.</param>
		/// <returns>The item with the specified key from the cache or the item returned by the loader.</returns>
		T GetOrAdd<T>(object key, Func<T> loader, Func<T, bool> shouldCache, params Type[] dependencies);

		/// <summary>
		/// Expires the item by the key
		/// </summary>
		/// <typeparam name="T">Item type</typeparam>
		/// <param name="key">The key</param>
		void Expire(object key);

		/// <summary>
		/// Remove all cached items with a specified type defined as a dependency.
		/// </summary>
		/// <param name="t"></param>
		void Modified(Type t);
	}
}
