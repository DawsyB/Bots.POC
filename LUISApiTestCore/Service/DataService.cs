using System;
using System.Collections.Generic;
using System.Text;
using LUISApiTestCore.Model;
using LUISApiTestCore.Service.Interface;

namespace LUISApiTestCore.Service
{
	class DataService : IDataService
	{
		public List<LuisIntent> GetIntents()
		{
			return new List<LuisIntent>() {
				new LuisIntent() { name = "DawsonTestIntent1", appId = Constants.LUISDefaultAppId, versionId = Constants.LUISVersion },
				new LuisIntent() { name = "DawsonTestIntent2",appId = Constants.LUISDefaultAppId, versionId = Constants.LUISVersion },
				new LuisIntent() { name = "DawsonTestIntent3",appId = Constants.LUISDefaultAppId, versionId = Constants.LUISVersion }
				};
		}

		public LuisApp GetLuisApp()
		{
			return new LuisApp()
			{
				name = "TestApp",
				description = "Test description",
				culture = "en-us",
				usageScenario = "",
				domain = "",
				initialVersionId = Constants.LUISVersion,
				versionId = Constants.LUISVersion,
				appId = Constants.LUISDefaultAppId
			};
		}

		

		public List<LuisIntent> GetIntentsForUpdate()
		{
			return new List<LuisIntent>() {
				new LuisIntent() { name = "DawsonTestIntent5",appId = Constants.LUISDefaultAppId, versionId = Constants.LUISVersion, IntentId = new Guid("e98564bb-a488-4cea-8842-bdbdbf03178b") },
				//new LuisIntent() { name = "DawsonTestIntent6",appId = Constants.LUISDefaultAppId, versionId = Constants.LUISVersion, IntentId = new Guid("9a4b90ba-b82f-4f53-bffa-8c848b2ff393") },
				//new LuisIntent() { name = "DawsonTestIntent7",appId = Constants.LUISDefaultAppId, versionId = Constants.LUISVersion, IntentId = new Guid("48942510-bc13-487a-92dd-9071bc8b7aca") }
				};
		}
	}
}
