using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LoadQnAData
{
    public class DownloadContentOfKBV4
    {
		static string host = "https://westus.api.cognitive.microsoft.com";
		static string service = "/qnamaker/v4.0";
		static string method = "/knowledgebases/{0}/{1}/qna/";

		// NOTE: Replace this with a valid subscription key.
		static string key = "146e55f338e5444ebd100b324250436e";

		// NOTE: Replace this with a valid knowledge base ID.
		static string kb = "14fe3f28-20b5-4c46-8cd8-4950808b5318";

		// NOTE: Replace this with "test" or "prod".
		static string env = "test";

		static string PrettyPrint(string s)
		{
			return JsonConvert.SerializeObject(JsonConvert.DeserializeObject(s), Formatting.Indented);
		}

		async static Task<string> Get(string uri)
		{
			using (var client = new HttpClient())
			using (var request = new HttpRequestMessage())
			{
				request.Method = HttpMethod.Get;
				request.RequestUri = new Uri(uri);
				request.Headers.Add("Ocp-Apim-Subscription-Key", key);

				var response = await client.SendAsync(request);
				return await response.Content.ReadAsStringAsync();
			}
		}

		async static void GetQnA()
		{
			var method_with_id = String.Format(method, kb, env);
			var uri = host + service + method_with_id;
			Console.WriteLine("Calling " + uri + ".");
			var response = await Get(uri);
			Console.WriteLine(PrettyPrint(response));
			Console.WriteLine("Press any key to continue.");
		}

		public static void Process()
		{
			GetQnA();
			Console.ReadLine();
		}

	}
}
