using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using IoContent.Sdk.Interfaces;
using IoContent.Sdk.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IoContent.Sdk
{
	public class ContentClient : IContentClient
	{
		private bool m_disposed;

		private int m_cacheInvalidationSeconds;

		private readonly string m_apiUrl;

		private readonly WebClient m_webClient;

		public ContentClient(ContentClientBaseParameters contentClientBaseParameters)
			: this(
				apiVersion: contentClientBaseParameters.ApiVersion,
				subAccountkey: contentClientBaseParameters.SubAccountKey, 
				contentType: contentClientBaseParameters.ContentType, 
				apiEndpointUrl: contentClientBaseParameters.ApiEndpointUrl
			)
		{

		}

		public ContentClient(string apiVersion, string subAccountkey, string contentType, string apiEndpointUrl = null)
		{
			// Check API endpoint format where specified

			if (!string.IsNullOrWhiteSpace(apiEndpointUrl))
			{
				if (!apiEndpointUrl.EndsWith("/"))
				{
					apiEndpointUrl += "/";
				}
			}

			m_apiUrl = (apiEndpointUrl ?? "https://iocontent.com/") + "api/" + apiVersion + "/content/" + subAccountkey + "/" + contentType;

			m_webClient = new WebClient();
		}

		public IContentClient WithLocalCache(int cacheInvalidationSeconds)
		{
			m_cacheInvalidationSeconds = cacheInvalidationSeconds;

			return this;
		}

		public void ClearCache(string queryString = null)
		{
			if (queryString != null)
			{
				// Clear cache for single entry

				string cacheKey = GetCacheKey(queryString);

				HttpContext.Current.Cache.Remove(cacheKey);
			}
			else
			{
				// Clear the entire cache

				foreach (System.Collections.DictionaryEntry cacheEntry in HttpContext.Current.Cache)
				{
					// Only remove CMS API cache items - other .NET cache items
					// may exist in the cache which we do not want to remove

					if (((string) cacheEntry.Key).StartsWith(CacheKeyPrefix))
					{
						HttpContext.Current.Cache.Remove((string) cacheEntry.Key);
					}
				}
			}
		}

		/// <summary>
		/// Returns content entries as raw Json. Property names are camel case formatted
		/// </summary>
		/// <param name="queryString"></param>
		/// <returns></returns>
		public string GetJson(string queryString)
		{
			string jsonResponse = null;

			// No need to authenticate prior to cache call, since cache data
			// is stored on the client

			string cacheKey = GetCacheKey(queryString);

			if (m_cacheInvalidationSeconds > 0)
			{
				object cacheData = HttpContext.Current.Cache.Get(cacheKey);

				jsonResponse = (string) cacheData;
			}

			if (jsonResponse == null)
			{
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
								statusCodeString = ((int) response.StatusCode).ToString(CultureInfo.InvariantCulture);

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

				if (m_cacheInvalidationSeconds > 0)
				{
					// Using the cache http://msdn.microsoft.com/en-us/library/vstudio/18c1wd61(v=vs.100).aspx

					DateTime absoluteExpiration = DateTime.Now.AddSeconds(m_cacheInvalidationSeconds);

					HttpContext.Current.Cache.Insert(cacheKey, jsonResponse, null, absoluteExpiration, System.Web.Caching.Cache.NoSlidingExpiration);
				}
			}

			return jsonResponse;
		}

		/// <summary>
		/// Returns content entries as a list of dynamics. Properties are accessed using Pascal case formatting
		/// </summary>
		/// <param name="queryString"></param>
		/// <returns></returns>
		public IList<dynamic> Get(string queryString)
		{
			string jsonResponse = GetJson(queryString);

			dynamic result;

			// Serializer settings control casing of dynamic properties.
			// Where the data is persisted in pascal case, in the .NET API
			// we'd rather work with Pascal case, as the rest of the object was 
			// in pascal case

			var settings = new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Converters = new List<JsonConverter> { new CamelCaseToPascalCaseDynamicConverter() }
			};

			result = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(jsonResponse, settings); // JsonSerializer<T>.FromJson(jsonResponse);
			
			return result;
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
			return this.CacheKeyPrefix + Base64Encode(urlPath);
		}

		private string Base64Encode(string plainText)
		{
			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(plainTextBytes);
		}
	}
}
