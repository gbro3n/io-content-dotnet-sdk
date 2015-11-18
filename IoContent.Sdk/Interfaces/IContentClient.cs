using System;
using System.Collections.Generic;

namespace IoContent.Sdk.Interfaces
{
	public interface IContentClient : IDisposable
	{
		string GetJson(string queryString);

		IList<dynamic> Get(string requestUrl);

		IContentClient WithLocalCache(int cacheInvalidationSeconds);
	}
}
