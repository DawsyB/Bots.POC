﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LUISApiTestCore
{
	public static class Constants
	{
		public const string LUISDefaultAppId = "6424ae0a-69fc-41a7-9f53-1917412a3fbe";
		public const string ProgrammaticKey = "50b45f2ccf7147e5969b897f2be08944";
		public const string LUISVersion = "1.0";
		public const string LUISAppRegion = "australiaeast";
		public const string LUISAppURL = "https://australiaeast.api.cognitive.microsoft.com/luis/api/v2.0/apps/?";
		public const string LUISIntentURL = "https://australiaeast.api.cognitive.microsoft.com/luis/api/v2.0/apps/{0}/versions/{1}/intents?";
		public const string LUISIntentUpdateURL = "https://australiaeast.api.cognitive.microsoft.com/luis/api/v2.0/apps/{0}/versions/{1}/intents/{2}";
		public const string LUISTrainURL = "https://australiaeast.api.cognitive.microsoft.com/luis/api/v2.0/apps/{0}/versions/{1}/train";
		public const string LUISPublishURL = "https://australiaeast.api.cognitive.microsoft.com/luis/api/v2.0/apps/{0}/publish";
		public const string LUISTrainStatusURL = "https://australiaeast.api.cognitive.microsoft.com/luis/api/v2.0/apps/{0}/versions/{1}/train";

		public enum RequestType{
			post,
			get,
			put

		}
	}
}
