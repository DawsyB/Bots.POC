using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LUISApiTestCore.Model
{
	public class LuisApp
	{
		/*public string name = "MyFirstDummyApp";
		public string description = "This is my first dummy application";
		public string culture = "en-us";
		public string usageScenario = "IoT";
		public string domain = "Comics";
		public string initialVersionId = "1.0";
		*/
		public string name { get; set; }
		public string description { get; set; }
		public string culture { get; set; }
		public string usageScenario { get; set; }
		public string domain { get; set; }
		public string initialVersionId { get; set; }
		[JsonIgnore]
		public string appId { get; set; }
		[JsonIgnore]
		public string versionId { get; set; }



	}

	
	
	
}
