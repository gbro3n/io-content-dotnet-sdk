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
			// Optionally pull content server side for example purposes

			var contentClientBaseParameters = new ContentClientBaseParameters
			{
				ApiVersion = "v1.0",
				ApiEndpointUrl = "https://iocontent.com",
				SubAccountKey = "rvlzpmb7koytevscusj2f4ntpc",
				ContentType = "test-article-a"
			};

			dynamic content;

			using (var contentClient = new ContentClient(contentClientBaseParameters))
			{
				contentClient.ClearCache();

				var contentList = contentClient.Get("?key.equals=r23gmukdmnbuuowk3ugrvxagac&markdownToHtml=true");

				content = contentList.FirstOrDefault();
			}

			Assert.NotNull(content);
			Assert.NotNull(content.Title);
			Assert.NotNull(content.Content);
		}

		[Test]
		public void test_retrieve_basic_content_with_local_cache()
		{
			// Optionally pull content server side for example purposes

			var contentClientBaseParameters = new ContentClientBaseParameters
			{
				ApiVersion = "v1.0",
				ApiEndpointUrl = "https://iocontent.com",
				SubAccountKey = "rvlzpmb7koytevscusj2f4ntpc",
				ContentType = "test-article-a"
			};

			dynamic content;

			using (var contentClient = new ContentClient(contentClientBaseParameters))
			{
				contentClient.ClearCache();

				var contentList = contentClient.WithLocalCache(30).Get("?key.equals=r23gmukdmnbuuowk3ugrvxagac&markdownToHtml=true");

				content = contentList.FirstOrDefault();
			}

			Assert.NotNull(content);
			Assert.NotNull(content.Title);
			Assert.NotNull(content.Content);
		}
	}
}
