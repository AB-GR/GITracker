using SQLite;
using SQLiteNetExtensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GITracker.Model
{
	public abstract class RepositoryBase
	{
		private static object syncPoint = new object();
		protected virtual SQLiteConnection Connection { get; }

		protected readonly ICache Cache;

		protected RepositoryBase()
		{
			Cache = new ModelCache();
		}

		protected void ExecuteWithRetry(Action lambda)
		{
			int counter = 0;
			while (true)
			{
				counter++;
				try
				{
					lock (syncPoint)
					{
						lambda();
					}
					return;
				}
				catch (SQLiteException ex)
				{
					if (counter >= 8 || ex.Result != SQLite3.Result.Busy)
						throw;
				}

				Task.Delay(80);
			}
		}

		protected T ExecuteWithRetry<T>(Func<T> lambda)
		{
			int counter = 0;
			while (true)
			{
				counter++;
				try
				{
					lock (syncPoint)
					{
						return lambda();
					}
				}
				catch (SQLiteException ex)
				{
					if (counter >= 8 || ex.Result != SQLite3.Result.Busy)
						throw;
				}

				Task.Delay(80);
			}
		}

		public int ExecuteNonQuery<T>(string sql, params object[] args)
		=> ExecuteWithRetry(() =>
		{
			var result = Connection.Execute(sql, args);
			Cache.Modified(typeof(T));
			return result;
		});

		public void Insert<T>(T obj)
		{
			ExecuteWithRetry(() =>
			{
				Connection.Insert(obj);
				Cache.Modified(typeof(T));
			});
		}

		public void InsertAll<T>(IEnumerable<T> list)
		{
			ExecuteWithRetry(() =>
			{
				Connection.InsertAll(list);
				Cache.Modified(typeof(T));
			});
		}

		public void Update<T>(T obj)
		{
			ExecuteWithRetry(() =>
			{
				Connection.Update(obj);
				Cache.Modified(typeof(T));
			});
		}

		public void UpdateAll<T>(IEnumerable<T> list)
		{
			ExecuteWithRetry(() =>
			{
				Connection.UpdateAll(list);
				Cache.Modified(typeof(T));
			});
		}

		public void Delete<T>(Guid id)
		{
			ExecuteWithRetry(() =>
			{
				Connection.Delete<T>(id);
				Cache.Modified(typeof(T));
			});
		}

		public void EmptyTable<T>()
		{
			ExecuteWithRetry(() =>
			{
				Connection.DeleteAll<T>();
				Cache.Modified(typeof(T));
			});
		}

		public void Delete<T>(Expression<Func<T, bool>> filter) where T : class, new()
		{
			ExecuteWithRetry(() =>
			{
				Connection.Table<T>().Delete(filter);
				Cache.Modified(typeof(T));
			});
		}

		public void RunInTransaction(Action action)
		=> ExecuteWithRetry(() => Connection.RunInTransaction(action));

		public List<T> ExecuteQuery<T>(string sql, params object[] args) where T : class, new()
		{
			lock (syncPoint)
			{
				return Connection.Query<T>(sql, args);
			}
		}

		public List<T> All<T>(bool includeChildren = false) where T : class, new()
		{
			lock (syncPoint)
			{
				return includeChildren
				? Connection.GetAllWithChildren<T>()
								: Connection.Table<T>().ToList();
			}
		}

		public List<T> All<T>(Expression<Func<T, bool>> filter, bool includeChildren = false) where T : class, new()
		{
			if (filter == null)
				throw new ArgumentNullException(nameof(filter));

			lock (syncPoint)
			{
				return includeChildren
					? Connection.GetAllWithChildren<T>(filter)
									: Connection.Table<T>().Where(filter).ToList();
			}
		}

		public List<T> AllOptimized<T>(Expression<Func<T, bool>> filter) where T : class, new()
		{
			if (filter == null)
				throw new ArgumentNullException(nameof(filter));

			lock (syncPoint)
			{
				return Connection.GetAllWithChildrenOptimized<T>(filter);
			}
		}

		public TResult Query<TModel, TResult>(Func<TableQuery<TModel>, TResult> queryExpr) where TModel : class, new()
		{
			lock (syncPoint)
			{
				return queryExpr(Connection.Table<TModel>());
			}
		}

		public T First<T>(bool includeChildren = false) where T : class, new()
		{
			var result = FirstOrDefault<T>(includeChildren);

			if (result == null)
				throw new NoRowException(typeof(T));

			return result;
		}

		public T First<T>(Expression<Func<T, bool>> filter, bool includeChildren = false) where T : class, new()
		{
			var result = FirstOrDefault<T>(filter, includeChildren);

			if (result == null)
				throw new NoRowException(typeof(T), $"For filter: {filter}.");

			return result;
		}

		public T First<T>(object pk, bool includeChildren = false) where T : class, new()
		{
			var result = FirstOrDefault<T>(pk, includeChildren);

			if (result == null)
				throw new NoRowException(typeof(T), $"For pk: {pk}.");

			return result;
		}

		public T FirstOrDefault<T>(bool includeChildren = false) where T : class, new()
		{
			lock (syncPoint)
			{
				return includeChildren
				? GetWithChildren<T>()
								: Connection.Table<T>().FirstOrDefault();
			}
		}

		public T FirstOrDefault<T>(Expression<Func<T, bool>> filter, bool includeChildren = false) where T : class, new()
		{
			if (filter == null)
				throw new ArgumentNullException(nameof(filter));

			lock (syncPoint)
			{
				return includeChildren
				? GetWithChildren<T>(filter)
								: Connection.Table<T>().Where(filter).FirstOrDefault();
			}
		}

		public T FirstOrDefault<T>(object pk, bool includeChildren = false) where T : class, new()
		{
			if (pk.IsNullOrDefault())
				throw new ArgumentNullException($"{pk} isn't valid.");

			lock (syncPoint)
			{
				return includeChildren
				? Connection.FindWithChildren<T>(pk)
								: Connection.Find<T>(pk);
			}
		}

		private T GetWithChildren<T>(Expression<Func<T, bool>> filter = null)
			where T : class, new()
		{
			var query = Connection.Table<T>();
			if (filter != null)
				query = query.Where(filter);

			var element = query.FirstOrDefault();

			if (element != null)
				Connection.GetChildren(element, false);

			return element;
		}
	}
}