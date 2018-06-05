using SQLite;
using SQLiteNetExtensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Gluco_SQLiteNetExtensions
{
	public static class ReadOperationsAsync
	{
		#region Public API

		/// <summary>
		/// Fetches all the entities of the specified type with the filter and fetches all the relationship
		/// properties of all the returned elements.
		/// </summary>
		/// <returns>List of all the elements of the type T that matches the filter with the children already loaded</returns>
		/// <param name="conn">SQLite Net connection object</param>
		/// <param name="filter">Filter that will be passed to the <c>Where</c> clause when fetching
		/// objects from the database. No relationship properties are allowed in this filter as they
		/// are loaded afterwards</param>
		/// <param name="recursive">If set to <c>true</c> all the relationships with
		/// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
		public static Task<List<T>> GetAllWithChildrenAsync<T>(this SQLiteAsyncConnection conn, Expression<Func<T, bool>> filter = null, bool recursive = false, CancellationToken cancellationToken = default(CancellationToken))
			where T : new()
		{
			return Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();

				var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
				using (connectionWithLock.Lock())
				{
					cancellationToken.ThrowIfCancellationRequested();
					return connectionWithLock.GetAllWithChildren(filter, recursive);
				}
			}, cancellationToken);
		}

		/// <summary>
		/// The behavior is the same that <c>GetWithChildren</c> but it returns null if the object doesn't
		/// exist in the database instead of throwing an exception
		/// Obtains the object from the database and fetch all the properties annotated with
		/// any subclass of <c>RelationshipAttribute</c>. If the object with the specified primary key doesn't
		/// exist in the database, it will return null
		/// </summary>
		/// <returns>The object with all the children loaded or null if it doesn't exist</returns>
		/// <param name="conn">SQLite Net connection object</param>
		/// <param name="pk">Primary key for the object to search in the database</param>
		/// <param name="recursive">If set to <c>true</c> all the relationships with
		/// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
		public static Task<T> FindWithChildrenAsync<T>(this SQLiteAsyncConnection conn, object pk, bool recursive = false, CancellationToken cancellationToken = default(CancellationToken))
			where T : new()
		{
			return Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();

				var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
				using (connectionWithLock.Lock())
				{
					cancellationToken.ThrowIfCancellationRequested();
					return connectionWithLock.FindWithChildren<T>(pk, recursive);
				}
			}, cancellationToken);
		}

		/// <summary>
		/// Fetches all the properties annotated with any subclass of <c>RelationshipAttribute</c> of the current
		/// object and keeps fetching recursively if the <c>recursive</c> flag has been set.
		/// </summary>
		/// <param name="conn">SQLite Net connection object</param>
		/// <param name="element">Element used to load all the relationship properties</param>
		/// <param name="recursive">If set to <c>true</c> all the relationships with
		/// <c>CascadeOperation.CascadeRead</c> will be loaded recusively.</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <typeparam name="T">Entity type where the object should be fetched from</typeparam>
		public static Task GetChildrenAsync<T>(this SQLiteAsyncConnection conn, T element, bool recursive = false, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Task.Run(() =>
			{
				cancellationToken.ThrowIfCancellationRequested();

				var connectionWithLock = SqliteAsyncConnectionWrapper.Lock(conn);
				using (connectionWithLock.Lock())
				{
					cancellationToken.ThrowIfCancellationRequested();
					connectionWithLock.GetChildren(element, recursive);
				}
			}, cancellationToken);
		}

		#endregion

	}

	public static class SqliteAsyncConnectionWrapper
	{
		private static readonly MethodInfo GetConnectionMethodInfo = typeof(SQLiteAsyncConnection).GetTypeInfo().GetDeclaredMethod("GetConnection");

		static public SQLiteConnectionWithLock Lock(SQLiteAsyncConnection asyncConnection)
		{
			return GetConnectionWithLock(asyncConnection);
		}

		static private SQLiteConnectionWithLock GetConnectionWithLock(SQLiteAsyncConnection asyncConnection)
		{
			return (SQLiteConnectionWithLock)GetConnectionMethodInfo.Invoke(asyncConnection, null);
		}
	}
}
