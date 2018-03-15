using System;
using System.Collections.Generic;
using System.Text;
using LUISApiTestCore.Model;

namespace LUISApiTestCore.Service.Interface
{
    interface IDataService
    {
		LuisApp GetLuisApp();
		List<LuisIntent> GetIntents();
		List<LuisIntent> GetIntentsForUpdate();

		
	}
}
