using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using LUISApiTestCore.LUIS;
using LUISApiTestCore.Model;
using LUISApiTestCore.Service;
using LUISApiTestCore.Service.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LUISApiTestCore
{
	class Program
	{
		static void Main(string[] args)
		{

			LuisFunctions().GetAwaiter().GetResult();


			Console.WriteLine("Hit ENTER to exit...");
			Console.ReadLine();
		}

		private static async Task LuisFunctions()
		{
			ILUISServices luisservice = new LuisService();
			IDataService dataservice = new DataService();

			//Create LUIS APP
			//await CreateLuisApp(luisservice, dataservice);


			//Add LUIS Intent
			//await AddIntents(luisservice, dataservice);

			//Update LUIS Intent
			// Daws: Doesn't work gives out resource not found
			//Needs investigation
			await UpdateIntents(luisservice, dataservice);

			// Train LUIS
			bool IsTrainingComplete = await TrainLuis(luisservice, dataservice);

			//Publish LUIS
			if (IsTrainingComplete)
				await PublishLuis(luisservice, dataservice);
			else
				Console.WriteLine("Publishing the App is on hold since training didn't complete successfully");
		}




		#region LUIS App
		private static async Task CreateLuisApp(ILUISServices luisservice, IDataService dataService)
		{
			var response = await luisservice.CreateLuisApp(dataService.GetLuisApp());
			if (response.IsSuccess)
				Console.WriteLine("The app is created successfully!!");
			else
				Console.WriteLine($"Issues with app creation: {response.Errorcode} Details: {response.message}");

		}

		private static async Task<bool> TrainLuis(ILUISServices luisservice, IDataService dataservice)
		{
			var app = dataservice.GetLuisApp();
			var response = await luisservice.TrainLuis(app);
			var converter = new ExpandoObjectConverter();
			dynamic results = JsonConvert.DeserializeObject<ExpandoObject>(response.ResponseMessage, converter);
			bool IsTrainingComplete = false;
			int StatusId = Int32.Parse(results.statusId.ToString());
			if (response.IsSuccess)
			{
				if (StatusId == 9)
				{
					Console.WriteLine($"Train request completed successfully.  Current queue status is {StatusId}");
					IsTrainingComplete = await WaitUntilTrainCompletes(luisservice, dataservice, app);
					Thread.Sleep(1000);
				}
				else if (StatusId == 2)
				{
					Console.WriteLine($"Luis App is upto date with training.");
				}
			}
			else
			{
				Console.WriteLine($"Error: Train request failed. Error: {response.Errorcode}. Details: {response.message}");
			}
			return IsTrainingComplete;
		}
		private static async Task PublishLuis(ILUISServices luisservice, IDataService dataService)
		{
			var response = await luisservice.PublishLuis(dataService.GetLuisApp());

			if (response.IsSuccess)
				Console.WriteLine("Publish request completed successfully.");
			else
				Console.WriteLine($"Error: Publish request failed. Error: {response.Errorcode}. Details: {response.message}");
		}

		private static async Task<bool> WaitUntilTrainCompletes(ILUISServices luisservice, IDataService dataservice, LuisApp app)
		{
			Thread.Sleep(10000);
			var response = await luisservice.GetTrainLuisStatus(app);

			var converter = new ExpandoObjectConverter();
			dynamic results = JsonConvert.DeserializeObject<IEnumerable<dynamic>>(response.ResponseMessage, converter);
			bool trainingComplete = true;
			foreach (dynamic result in results)
			{
				var status = result.details.status.ToString();
				switch (status)
				{
					case "InProgress":
						Console.WriteLine($"ModelId:{result.modelId}. Status:{result.details.statusId}-{status}. ExampleCount: {result.details.exampleCount}");
						break;
					case "Fail":
						Console.WriteLine($"ModelId:{result.modelId}. Status:{result.details.statusId}-{status}. Failure reason: {result.details.failureReason} ExampleCount: {result.exampleCount}");
						break;
					case "UpToDate":
						Console.WriteLine($"ModelId:{result.modelId}. Status:{result.details.statusId}-{status}. ExampleCount: {result.details.exampleCount}. Training TimeDate: {result.details.trainingDateTime}");
						break;
					case "Success":
						Console.WriteLine($"ModelId:{result.modelId}. Status:{result.details.statusId}-{status}. ExampleCount: {result.details.exampleCount}. Training TimeDate: {result.details.trainingDateTime}");
						break;
				}
				if (status == "InProgress" || status == "Fail")
				{
					trainingComplete = false;
					break;
				}
			}
			if (trainingComplete)
			{
				Console.WriteLine("Training completed");
			}
			else
			{
				Console.WriteLine($"Error found. Training could not be completed. Please check if all the intents have utterances");
				// Uncomment the below method to re run the same method again until it finishes
				//await WaitUntilTrainCompletes(luisservice, dataservice, app);
			}
			return trainingComplete;
		}
		#endregion

		#region LUIS Intent
		private static async Task AddIntents(ILUISServices luisservice, IDataService dataService)
		{
			var response = await luisservice.AddIntents(dataService.GetIntents());
			if (response.Count > 0)
			{
				var intents = response.FindAll(c => c.IsSuccess == false);
				if (intents.Count == 0)
					Console.WriteLine("All intents were created successfully");
				else
				{
					foreach (IntentServiceResponse intent in intents)
					{
						Console.WriteLine($"Failed Intent: {intent.Intent}, Error message: {intent.serviceresponse.Errorcode} Details: {intent.serviceresponse.message} ");
					}
				}
			}

		}

		private static async Task UpdateIntents(ILUISServices luisService, IDataService dataService)
		{
			var response = await luisService.UpdateIntents(dataService.GetIntentsForUpdate());
			if (response.Count > 0)
			{
				var intents = response.FindAll(c => c.IsSuccess == false);
				if (intents.Count == 0)
					Console.WriteLine("All intents were updated successfully");
				else
				{
					foreach (IntentServiceResponse intent in intents)
					{
						Console.WriteLine($"Failed Intent: {intent.Intent}, Error message: {intent.serviceresponse.Errorcode} Details: {intent.serviceresponse.message} ");
					}
				}
			}
		}
		#endregion
	}
}
