using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace LUISApiTestCore.Model
{
    class LuisIntent
    {
		public string name { get; set; }
		[JsonIgnore]
		public string appId { get; set; }
		[JsonIgnore]
		public string versionId { get; set; }
		[JsonIgnore]
		public Guid IntentId { get; set; }
	}
	
}
