using System;
using System.Web;
using System.Web.Caching;
using IoContent.Sdk.Interfaces;

namespace IoContent.Sdk
{
	internal class MemoryCacheService : IMemoryCacheService
	{
		public T Get<T>(string cacheKey, Func<T> getCacheObjCallback, DateTime? absoluteExpirationDate = null, TimeSpan? slidingExpiration = null) where T : class
		{
			var cacheObj = HttpRuntime.Cache.Get(cacheKey) as T;

			if (cacheObj == null)
			{
				cacheObj = getCacheObjCallback();

				HttpRuntime.Cache.Insert(
					cacheKey,
					cacheObj,
					null,
					absoluteExpirationDate.HasValue ? (DateTime) absoluteExpirationDate : Cache.NoAbsoluteExpiration,
					slidingExpiration.HasValue ? (TimeSpan) slidingExpiration : Cache.NoSlidingExpiration
				);
			}

			return cacheObj;
		}

		public void Remove(string cacheKey)
		{
			HttpRuntime.Cache.Remove(cacheKey);
		}

		public void RemoveAllWithPrefix(string prefix)
		{
			foreach (System.Collections.DictionaryEntry cacheEntry in HttpRuntime.Cache)
			{
				if (((string) cacheEntry.Key).StartsWith(prefix))
				{
					HttpRuntime.Cache.Remove((string) cacheEntry.Key);
				}
			}
		}
	}
}
