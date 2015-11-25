using System;
using System.Linq;
using NUnit.Framework;

namespace IoContent.Sdk.Tests
{
	[TestFixture]
	public class ContentClientTests
    {
		[Test]
		public void test_retrieve_basic_content_without_local_cache()
		{
			var contentClientBaseParameters = new ContentClientBaseParameters
			{
				ApiVersion = "v1.0",
				ApiEndpointUrl = "https://iocontent.com",
				SubAccountKey = "rvlzpmb7koytevscusj2f4ntpc",
				ContentType = "test-article-a"
			};

			dynamic response;

			using (var contentClient = new ContentClient(contentClientBaseParameters))
			{
				contentClient.ClearCache();

				response = contentClient.Get("?key.equals=r23gmukdmnbuuowk3ugrvxagac&markdownToHtml=true");
			}

			Assert.NotNull(response);

			Assert.NotNull(response.data[0].title);
			Assert.NotNull(response.data[0].content);
			Assert.NotNull(response.data[0].createdDate);

			Assert.AreEqual(typeof(string), response.data[0].title.GetType());
			Assert.AreEqual(typeof(string), response.data[0].content.GetType());
			Assert.AreEqual(typeof(DateTime), response.data[0].createdDate.GetType());

			Assert.NotNull(response.page);
			Assert.NotNull(response.metaData.responseId);
			Assert.NotNull(response.metaData.lastModified);
		}

		[Test]
		public void test_retrieve_basic_content_with_local_cache()
		{
			// Note: HttpRuntime caching is not available in unit tests
			// so while the API is tested here, this is not a true cache test

			var contentClientBaseParameters = new ContentClientBaseParameters
			{
				ApiVersion = "v1.0",
				ApiEndpointUrl = "https://iocontent.com",
				SubAccountKey = "rvlzpmb7koytevscusj2f4ntpc",
				ContentType = "test-article-a"
			};

			dynamic response;

			using (var contentClient = new ContentClient(contentClientBaseParameters))
			{
				contentClient.ClearCache();

				response = contentClient.WithLocalCache(60).Get("?key.equals=r23gmukdmnbuuowk3ugrvxagac&markdownToHtml=true");
			}

			Assert.NotNull(response);
			Assert.NotNull(response.data[0].title);
			Assert.NotNull(response.data[0].content);
			Assert.NotNull(response.data[0].createdDate);

			Assert.AreEqual(typeof(string), response.data[0].title.GetType());
			Assert.AreEqual(typeof(string), response.data[0].content.GetType());
			Assert.AreEqual(typeof(DateTime), response.data[0].createdDate.GetType());

			Assert.NotNull(response.page);
			Assert.NotNull(response.metaData.responseId);
			Assert.NotNull(response.metaData.lastModified);
		}
	}
}
