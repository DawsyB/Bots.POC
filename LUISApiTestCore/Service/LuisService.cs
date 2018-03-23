using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using LUISApiTestCore.Model;
using LUISApiTestCore.Service.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace LUISApiTestCore.Service
{
	class LuisService : ILUISServices
	{
		#region Create Luis App
		public async Task<ServiceResponse> CreateLuisApp(LuisApp App)
		{
			var body = JsonConvert.SerializeObject(App);
			return GetServiceResponse(await GetResponse(body, Constants.LUISAppURL,Constants.RequestType.post.ToString()));

		}

		public async Task<ServiceResponse> TrainLuis(LuisApp App)
		{
			return GetServiceResponse(await GetResponse(string.Empty, string.Format(Constants.LUISTrainURL, App.appId, App.versionId), Constants.RequestType.post.ToString()));
		}

		public async Task<ServiceResponse> GetTrainLuisStatus(LuisApp App)
		{
			return GetServiceResponse(await GetResponse(string.Format(Constants.LUISTrainStatusURL, App.appId, App.versionId), Constants.RequestType.post.ToString()));
		}
		public async Task<ServiceResponse> PublishLuis(LuisApp App)
		{
			dynamic bodyobj = new ExpandoObject();
			bodyobj.versionId = App.versionId;
			bodyobj.isStaging = false;
			bodyobj.region = Constants.LUISAppRegion;
			var body = JsonConvert.SerializeObject(bodyobj) ;
			return GetServiceResponse(await GetResponse(body, string.Format(Constants.LUISPublishURL, App.appId), Constants.RequestType.post.ToString()));
		}
		#endregion

		#region Intents

		public async Task<List<IntentServiceResponse>> AddIntents(List<LuisIntent> intentsList)
		{
			List<IntentServiceResponse> IntentResponseList = new List<IntentServiceResponse>();
			foreach (LuisIntent intent in intentsList)
			{
				IntentServiceResponse IntentResponse = new IntentServiceResponse();
				var body = JsonConvert.SerializeObject(intent);
				var response = GetServiceResponse(await GetResponse(body, string.Format(Constants.LUISIntentURL, intent.appId, intent.versionId), Constants.RequestType.put.ToString()));
				IntentResponse.IsSuccess = response.IsSuccess;
				IntentResponse.serviceresponse = response;
				IntentResponse.Intent = intent.name;
				IntentResponseList.Add(IntentResponse);
			}
			return IntentResponseList;
		}

		public async Task<List<IntentServiceResponse>> UpdateIntents(List<LuisIntent> intentsList)
		{
			List<IntentServiceResponse> IntentResponseList = new List<IntentServiceResponse>();
			foreach (LuisIntent intent in intentsList)
			{
				IntentServiceResponse IntentResponse = new IntentServiceResponse();
				var body = JsonConvert.SerializeObject(intent);
				var response = GetServiceResponse(await GetResponse(body, string.Format(Constants.LUISIntentUpdateURL, intent.appId, intent.versionId, intent.IntentId)));
				IntentResponse.IsSuccess = response.IsSuccess;
				IntentResponse.serviceresponse = response;
				IntentResponse.Intent = intent.name;
				IntentResponseList.Add(IntentResponse);
			}
			return IntentResponseList;
		}
		#endregion

		#region common
		/// <summary>
		/// Gets Response with passing body to the API call
		/// </summary>
		/// <param name="data"></param>
		/// <param name="ApiUrl"></param>
		/// <returns></returns>
		public async Task<HttpResponseMessage> GetResponse(string data, string ApiUrl,string requestType)
		{
			try
			{

				var client = new HttpClient();
				var queryString = HttpUtility.ParseQueryString(string.Empty);

				// Request headers
				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.ProgrammaticKey);

				var uri = ApiUrl + queryString;

				HttpResponseMessage response;
				byte[] byteData = Encoding.UTF8.GetBytes(data);
				using (var content = new ByteArrayContent(byteData))
				{
					content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
					if(requestType == "post")
					response = await client.PostAsync(uri, content);
					else if (requestType == "put")
					response = await client.PutAsync(uri, content);
				}
				return response;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return null;
			}
		}
		/// <summary>
		/// Get Response without passing Data
		/// </summary>
		/// <param name="ApiUrl"></param>
		/// <returns></returns>
		private async Task<HttpResponseMessage> GetResponse(string ApiUrl)
		{
			var client = new HttpClient();
			var queryString = HttpUtility.ParseQueryString(string.Empty);

			// Request headers
			client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Constants.ProgrammaticKey);

			var uri = ApiUrl + queryString;

			return  await client.GetAsync(uri);
		}
		private ServiceResponse GetServiceResponse(HttpResponseMessage response)
		{
			ServiceResponse serviceResponse = new ServiceResponse();
			serviceResponse.ResponseMessage = response.Content.ReadAsStringAsync().Result;
			serviceResponse.StatusCode = response.StatusCode;
			serviceResponse.IsSuccess = response.IsSuccessStatusCode;
			if (!serviceResponse.IsSuccess)
			{
				var converter = new ExpandoObjectConverter();
				var obj = response.Content.ReadAsStringAsync().Result;
				dynamic results = JsonConvert.DeserializeObject<ExpandoObject>(obj, converter);
				if (((IDictionary<string, object>)results).ContainsKey("error"))
				{
					serviceResponse.Errorcode = results.error.code.ToString();
					serviceResponse.message = results.error.message.ToString();
				}
				// Intents update throws gives error message in a different format
				else if (((IDictionary<string, object>)results).ContainsKey("statusCode"))
				{
					serviceResponse.Errorcode = results.statusCode.ToString();
					serviceResponse.message = results.message.ToString();
				}
			}
			return serviceResponse;
		}

		


		#endregion
	}
}
