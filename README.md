# I/O Content .NET SDK (API Wrapper)

The I/O Content SDK assists with making calls to the [iocontent.com](http://www.iocontent.com) REST content API.

An installation package is available via NuGet

![Install-Package IoContent.Sdk via nuget](https://cdn.iocontent.com/v1.0/assets/nfm6dwvsmrd6uukgj3rzdugerc/20151110-140426333/64dr/iocontent-nuget.png)

## Usage

A simple example is shown below - The sub account and content type are set before requesting content via the API. In this example the  query string `key.equals=r23gmukdmnbuuowk3ugrvxagac&markdownToHtml=true` causes the API to return content with the given key and convert markdown fields to HTML.

Further methods and properties available on the ContentClient class are documented below.

Full documentation for I/O Content and the API can be found [here](https://iocontent.com/documentation).


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
	
		dynamic contentResponse;
	
		using (var contentClient = new ContentClient(contentClientBaseParameters))
		{
			contentClient.ClearCache();
	
			contentResponse = contentClient.WithLocalCache(30).Get("?key.equals=r23gmukdmnbuuowk3ugrvxagac&markdownToHtml=true");
		}
	
		if (contentResponse != null)
		{
			ViewBag.Title = contentResponse.data[0].title;
		}
	}
	
	@section Content {
	
		<div>
	
			@{
				if (contentResponse != null)
				{
					<div>@Html.Raw(contentResponse.data[0].content)</div>
				}
			}
	
		</div>
	}





## Constructors

### ContentClient(ContentClientBaseParameters contentClientBaseParameters)

Basic constructor. Specify sub account and content type as a minimum.

### ContentClient(ContentClientBaseParameters contentClientBaseParameters, IMemoryCacheService memoryCacheService)

Overloaded constructor. A custom implementation of IMemoryCacheService can be passed where `HttpRuntime.Cache` is not available.

## Methods

### IList<dynamic> Get(string queryString)

Parses the content response into a list of dynamics. Properties are accessed by the content type field key (`content.title` or `content.content` in the example above). Note that the content response property names are camelCase as they are found in the raw JSON response. While .NET naming conventions usually use PascalProperty names, reformatting the property names introduces a performance overhead and so is not included in this SDK.

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

Read only access to CacheInvalidationSeconds set in argument to `.WithLocalCache`.

