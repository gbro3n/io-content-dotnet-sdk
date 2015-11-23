using System;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using IoContent.Sdk.Interfaces;
using Newtonsoft.Json;

namespace IoContent.Sdk
{
	public class ContentClient : IContentClient
	{
		private bool m_disposed;

		private string m_apiUrl;

		private WebClient m_webClient;

		private IMemoryCacheService m_memoryCacheService;

		public ContentClient(ContentClientBaseParameters contentClientBaseParameters)
		{
			this.Initialise(contentClientBaseParameters, new MemoryCacheService());
		}

		public ContentClient(ContentClientBaseParameters contentClientBaseParameters, IMemoryCacheService memoryCacheService)
		{
			this.Initialise(contentClientBaseParameters, memoryCacheService);
		}

		private void Initialise(ContentClientBaseParameters contentClientBaseParameters, IMemoryCacheService memoryCacheService)
		{
			if (!string.IsNullOrWhiteSpace(contentClientBaseParameters.ApiEndpointUrl))
			{
				if (!contentClientBaseParameters.ApiEndpointUrl.EndsWith("/"))
				{
					contentClientBaseParameters.ApiEndpointUrl += "/";
				}
			}

			string apiUrlPath = string.Format("api/{0}/content/{1}/{2}", contentClientBaseParameters.ApiVersion, contentClientBaseParameters.SubAccountKey, contentClientBaseParameters.ContentType);

			m_apiUrl = (contentClientBaseParameters.ApiEndpointUrl ?? "https://iocontent.com/") + apiUrlPath;

			m_webClient = new WebClient();

			m_memoryCacheService = memoryCacheService;
        }


		public int CacheInvalidationSeconds { get; private set; }

		public bool LastResponseWasFromCache { get; private set; }

		/// <summary>
		/// Set number of seconds that queries with the following querystring should
		/// cache responses for.
		/// </summary>
		/// <param name="cacheInvalidationSeconds"></param>
		/// <returns></returns>
		public IContentClient WithLocalCache(int cacheInvalidationSeconds)
		{
			CacheInvalidationSeconds = cacheInvalidationSeconds;

			return this;
		}

		/// <summary>
		/// Clear cache
		/// </summary>
		/// <param name="queryString">Querystring for which cache should be cleared (leave null to clear all)</param>
		public void ClearCache(string queryString = null)
		{
			if (queryString != null)
			{
				// Clear cache for single entry

				string cacheKey = GetCacheKey(queryString);

				m_memoryCacheService.Remove(cacheKey);
			}
			else
			{
				// Clear the entire cache

				m_memoryCacheService.RemoveAllWithPrefix(this.CacheKeyPrefix);
			}
		}

		/// <summary>
		/// Returns content entries as raw Json.
		/// </summary>
		/// <param name="queryString">Content query string.</param>
		/// <returns></returns>
		public string GetJson(string queryString)
		{
			this.LastResponseWasFromCache = true;

			// No need to authenticate prior to cache call, since cache data
			// is stored on the client

			string cacheKey = GetCacheKey(queryString);

			Func<string> getCacheObjCallback = () =>
			{
				string jsonResponse;

				// jsonResponse indicates that string has not been assigned from
				// cache, so call API to retrieve json

				if (!string.IsNullOrWhiteSpace(queryString))
				{
					if (!queryString.StartsWith("?"))
					{
						queryString = "?" + queryString;
					}
				}

				string apiRequestUrl = m_apiUrl + queryString;

				byte[] bytes;

				try
				{
					bytes = m_webClient.DownloadData(apiRequestUrl);
				}
				catch (WebException wEx)
				{
					// CMS api returns contextual information in response body which improves on standard
					// web exception text, so catch and extract response body, rethrowing an exception with a more useful message

					if (wEx.Response != null)
					{
						var responseStream = wEx.Response.GetResponseStream();

						if (responseStream != null)
						{
							var response = wEx.Response as HttpWebResponse;

							string statusCodeString = null;

							if (response != null)
							{
								statusCodeString = ((int)response.StatusCode).ToString(CultureInfo.InvariantCulture);

								statusCodeString = "(" + statusCodeString + ")";
							}

							string responseText = new StreamReader(responseStream).ReadToEnd();

							responseText = responseText.Replace("\"", string.Empty); // Remove " quotes

							responseText = string.Format("{0} {1}", statusCodeString, responseText);

							responseText = responseText.Trim();

							throw new WebException(responseText, wEx.Status);
						}

						// Rethrow as received

						throw wEx;
					}

					// Rethrow as received

					throw wEx;
				}

				jsonResponse = Encoding.UTF8.GetString(bytes);

				this.LastResponseWasFromCache = false;

				return jsonResponse;
			};

			string jsonStr;

			if (CacheInvalidationSeconds > 0)
			{
				jsonStr = m_memoryCacheService.Get(

					cacheKey,
					getCacheObjCallback,
					DateTime.Now.AddSeconds(CacheInvalidationSeconds)
				);
			}
			else
			{
				jsonStr = getCacheObjCallback();
            }

			return jsonStr;
		}

		/// <summary>
		/// Returns content entries as a list of dynamics. Properties are accessed using Pascal case formatting in line 
		/// with standard C# coding conventions.
		/// </summary>
		/// <param name="queryString">Content query string</param>
		/// <returns></returns>
		public dynamic Get(string queryString)
		{
			string jsonResponse = GetJson(queryString);

			dynamic response;

			//Removed use of CamelCaseToPascalCaseDynamicConverter. Preferred use
			// of ExpandoObject type for deserialization was incompatible with non essential
			// Pascal case property name formatting

			// Note that we deserialize using ExpandoObject - not dynamic. If we 
			// specify dynamic we get JObject back. If we use Expando the dynamic properties
			// are correct native .NET types

			response = JsonConvert.DeserializeObject<ExpandoObject>(jsonResponse);

			return response;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose(bool disposing)
		{
			if (!m_disposed)
			{
				if (disposing)
				{
					if (m_webClient != null)
					{
						m_webClient.Dispose();
					}
				}

				m_disposed = true;
			}
		}

		private string CacheKeyPrefix
		{
			get { return "iocontent_local_"; }
		}

		private string GetCacheKey(string urlPath)
		{
			return string.Format("{0}_" + "_{1}_{2}", this.CacheKeyPrefix, this.CacheInvalidationSeconds, Base64Encode(urlPath));
		}

		private string Base64Encode(string plainText)
		{
			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(plainTextBytes);
		}
	}
}
