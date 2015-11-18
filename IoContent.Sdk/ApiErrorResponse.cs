using System.Net;

namespace IoContent.Sdk
{
	public class ApiErrorResponse
	{
		public ApiErrorResponse(string errorMessage, HttpStatusCode httpStatusCode)
		{
			this.ErrorMessage = errorMessage;
			this.HttpStatusCode = httpStatusCode;
		}
			
		public string ErrorMessage { get; set; }

		public HttpStatusCode HttpStatusCode { get; set; }
	}
}
