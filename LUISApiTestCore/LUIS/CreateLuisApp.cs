using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;
using System.Threading.Tasks;
using LUISApiTestCore.Model;

namespace LUISApiTestCore.LUIS
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
				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "");
				

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


}
