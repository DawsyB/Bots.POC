﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LoadQnAData
{
	class Program
	{
		protected static string _KnowledgeBase = "75f0285d-80b5-464b-becc-2df87c3f77e8";
		protected static string _OcpApimSubscriptionKey = "95f32b854bee41d784b0b0215fe8cead";

		static void Main(string[] args)
		{

			MainAsync().GetAwaiter().GetResult(); ;
		}

		static async Task MainAsync()
		{
			// The GetFAQ button was pressed
			// Call the QnA Service and get the FAQ

			var strFAQ = await GetFAQ();
			Console.ReadLine();
			
		}

		static async Task<Dictionary<string,string>> GetFAQ()
		{
			try
			{
				string strFAQUrl = "";
				string strLine;
				StringBuilder sb = new StringBuilder();
				// Get the URL to the FAQ (in .tsv form)
				using (System.Net.Http.HttpClient client =
					new System.Net.Http.HttpClient())
				{
					string RequestURI = String.Format("{0}{1}{2}{3}{4}",
						@"https://westus.api.cognitive.microsoft.com/",
						@"qnamaker/v2.0/",
						@"knowledgebases/",
						_KnowledgeBase,
						@"? ");
					client.DefaultRequestHeaders.Add(
						"Ocp-Apim-Subscription-Key", _OcpApimSubscriptionKey);

					client.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue()
					{
						MaxAge = new TimeSpan(0, 0, 5, 0),
						MustRevalidate = true

					};

					System.Net.Http.HttpResponseMessage msg =
						await client.GetAsync(RequestURI);
					if (msg.IsSuccessStatusCode)
					{
						var JsonDataResponse =
							await msg.Content.ReadAsStringAsync();
						strFAQUrl =
							JsonConvert.DeserializeObject<string>(JsonDataResponse);
					}
				}
				// Make a web call to get the contents of the
				// .tsv file that contains the database
				var req = WebRequest.Create(strFAQUrl);
				
				var r = await req.GetResponseAsync().ConfigureAwait(false);
				// Read the response
				Dictionary<string, string> QnAContent = new Dictionary<string, string>();
				using (var responseReader = new StreamReader(r.GetResponseStream()))
				{
					int counter = 0;
					
					// Read through each line of the response
					while ((strLine = responseReader.ReadLine()) != null)
					{
						if (counter > 0)
						{
							// Write the contents to the StringBuilder object
							string[] strCurrentLine = strLine.Split('\t');
							QnAContent.Add(strCurrentLine[0], strCurrentLine[1]);
						}
						counter++;
					}
				}
				// Return the contents of the StringBuilder object
				return QnAContent;
			}
			catch (Exception ex)
			{
				return null;
			}
		}


	}
}
