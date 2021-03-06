﻿using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using Microsoft.Cognitive.LUIS;
using System.Web.Helpers;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LUISApiV2._0_Test.LUIS
{
	class CreateLuisApp
	{
		public static async Task<HttpResponseMessage> MakeRequest()
		{
			try
			{
				var app = JsonConvert.SerializeObject(new LuisApp());
				var client = new HttpClient();
				var queryString = HttpUtility.ParseQueryString(string.Empty);

				// Request headers
				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "50b45f2ccf7147e5969b897f2be08944");
				

				var uri = "https://australiaeast.api.cognitive.microsoft.com/luis/api/v2.0/apps/?" + queryString;

				HttpResponseMessage response;
				

				// Request body
				byte[] byteData = Encoding.UTF8.GetBytes(app);

				
				using (var content = new ByteArrayContent(byteData))
				{
					content.Headers.ContentType = new MediaTypeHeaderValue("application/json");					
					response = await client.PostAsync(uri, content);
				}
				return response;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
			
		}

		
	}

	public class LuisApp
	{
		public string name = "MyFirstDummyApp";
		public string description = "This is my first dummy application";
		public string culture = "en-us";
		public string usageScenario = "IoT";
		public string domain = "Comics";
		public string initialVersionId = "1.0";

	}
}
