using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LUISApiTestCore.Model;

namespace LUISApiTestCore.Service.Interface
{
    interface ILUISServices
    {
		Task<ServiceResponse> CreateLuisApp(LuisApp app);
		Task<List<IntentServiceResponse>> AddIntents(List<LuisIntent> intents);
		Task<List<IntentServiceResponse>>UpdateIntents(List<LuisIntent> intents);
		Task<ServiceResponse> TrainLuis(LuisApp app);
		Task<ServiceResponse> PublishLuis(LuisApp app);
		Task<ServiceResponse> GetTrainLuisStatus(LuisApp app);
	}
}
