# I/O Content .NET SDK (API Wrapper)

The I/O Content SDK assists with making calls to the [iocontent.com](http://www.iocontent.com) REST content API.

A simple example is as follows - The sub account and content type are set before requesting content via the API. In this example the  query string `key.equals=r23gmukdmnbuuowk3ugrvxagac&markdownToHtml=true` causes the API to return content with the given key and convert markdown fields to HTML.

Further methods and properties available on the ContentClient class are documented below.

Full documentation I/O Content and the API are [here](https://github.com/appsoftware/io-content-docs).

```
@using IoContent.Sdk

@{
	Layout = "~/Views/Shared/MainLayout.cshtml";
}

@model Dictionary<int, string>

@{
	// Optionally pull content server side for example purposes

	var contentClientBaseParameters = new ContentClientBaseParameters
	{
		SubAccountKey = "rvlzpmb7koytevscusj2f4ntpc",
		ContentType = "test-article-a"
	};

	dynamic content;

	using (var contentClient = new ContentClient(contentClientBaseParameters))
	{
		var contentList = contentClient.WithLocalCache(30).Get("?key.equals=r23gmukdmnbuuowk3ugrvxagac&markdownToHtml=true");

		content = contentList.FirstOrDefault();
	}

	if (content != null)
	{
		ViewBag.Title = content.Title;
	}
}

@section Content {

	<div>

		@{
			if (content != null)
			{
				<div>@Html.Raw(content.Content)</div>
			}
		}

	</div>
}
```

## Constructors

### ContentClient(ContentClientBaseParameters contentClientBaseParameters)

Basic constructor. Specify sub account and content type as a minimum.

### ContentClient(ContentClientBaseParameters contentClientBaseParameters, IMemoryCacheService memoryCacheService)

Overloaded constructor. A custom implementation of IMemoryCacheService can be passed where `HttpRuntime.Cache` is not available.

## Methods

### IList<dynamic> Get(string queryString, bool convertPropertyNamesToCamelCase = true)

Parses the content response into a list of dynamics. Properties are accessed by the content type field key (`content.Title` or `content.Content` in the example above). Note that content field keys are camelCase by default, but are converted to PascalCase in ContentClient.Get(). This behaviour can be overridden by setting `convertPropertyNamesToCamelCase=false`.

### string GetJson(string queryString)

Returns content response as a raw JSON string.

### IContentClient WithLocalCache(int cacheInvalidationSeconds)

Specify that responses should be stored in the local cache for the number of 
seconds specified in `cacheInvalidationSeconds`.

### void ClearCache(string queryString = null)

Clear the local cache for a given query string or pass null to clear the local cache completely.

## Properties

### LastResponseWasFromCache

Indicates whether the last request was served from the local cache (for debugging purposes).

### CacheInvalidationSeconds

Read only access to CacheInvalidationSeconds set.

