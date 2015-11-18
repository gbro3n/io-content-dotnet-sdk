namespace IoContent.Sdk
{
	public class ApiClientBaseParameters
	{
		public ApiClientBaseParameters() : this(apiVersion: "v1.0")
		{

		}

		public ApiClientBaseParameters(string apiVersion)
		{
			ApiVersion = apiVersion;
		}

		public string ApiVersion { get; set; }

		public string SubAccountKey { get; set; }

		public string ApiEndpointUrl { get; set; } = "https://iocontent.com/";
	}
}
