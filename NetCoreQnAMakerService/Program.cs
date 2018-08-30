using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NetCoreQnAMakerService
{
    class Program
    {
		// NOTE: Replace this with a valid host name.
		static string host = "https://utsdevqnamaker.azurewebsites.net/";

		// NOTE: Replace this with a valid endpoint key.
		// This is not your subscription key.
		// To get your endpoint keys, call the GET /endpointkeys method.
		static string endpoint_key = "6054693c-f5d0-4416-8c9c-bae906cd35ea";

		// NOTE: Replace this with a valid knowledge base ID.
		// Make sure you have published the knowledge base with the
		// POST /knowledgebases/{knowledge base ID} method.
		static string kb = "14fe3f28-20b5-4c46-8cd8-4950808b5318";

		static string service = "/qnamaker";
		static string method = "/knowledgebases/" + kb + "/generateAnswer/";

		static string question = @"
{
    'question': 'Fees_FeeHelp',
    'top': 3
}
";

		static void Main(string[] args)
		{
			GetAnswers();
			Console.ReadLine();
		}

		async static void GetAnswers()
		{
			var uri = host + service + method;
			Console.WriteLine("Calling " + uri + ".");
			var response = await Post(uri, question);
			Console.WriteLine(response);
			Console.WriteLine("Press any key to continue.");
		}
		async static Task<string> Post(string uri, string body)
		{
			using (var client = new HttpClient())
			using (var request = new HttpRequestMessage())
			{
				request.Method = HttpMethod.Post;
				request.RequestUri = new Uri(uri);
				request.Content = new StringContent(body, Encoding.UTF8, "application/json");
				request.Headers.Add("Authorization", "EndpointKey " + endpoint_key);

				var response = await client.SendAsync(request);
				var responsestring =  await response.Content.ReadAsStringAsync();
				var answer = JsonConvert.DeserializeObject<QnAResponseAnswer>(responsestring);
				return responsestring;
			}
		}
	}

	public class QnAResponseAnswer
	{
		public List<QnAAnswer> answers { get; set; }
	}
	public class QnAAnswer
	{
		public List<string> questions { get; set; }
		public string answer { get; set; }
		public double score { get; set; }
		public int id { get; set; }
		public string source { get; set; }
		public List<string> metadata { get; set; }
	}
}
