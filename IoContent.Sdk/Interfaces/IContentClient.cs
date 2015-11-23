using System;
using System.Collections.Generic;

namespace IoContent.Sdk.Interfaces
{
	public interface IContentClient : IDisposable
	{
		string GetJson(string queryString);

		dynamic Get(string requestUrl);

		IContentClient WithLocalCache(int cacheInvalidationSeconds);
	}
}
