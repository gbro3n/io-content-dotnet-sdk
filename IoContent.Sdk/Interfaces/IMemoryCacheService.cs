using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IoContent.Sdk.Interfaces
{
	public interface IMemoryCacheService
	{
		T Get<T>(string cacheKey, Func<T> getCacheObjCallback, DateTime? absoluteExpirationDate = null, TimeSpan? slidingExpiration = null) where T : class;

		void Remove(string cacheKey);

		void RemoveAllWithPrefix(string prefix);
    }
}
