using Newtonsoft.Json;

namespace IoContent.Sdk.Serialization
{
	internal static class JsonSerializer<T>
	{
		public static string ToJson(object obj, bool indented = true)
		{
			string json = JsonConvert.SerializeObject(obj, typeof(T), indented ? Formatting.Indented : Formatting.None, null);

			return json;
		}

		public static T FromJson(string jsonObjectString)
		{
			T obj = JsonConvert.DeserializeObject<T>(jsonObjectString);

			return obj;
		}
	}
}
