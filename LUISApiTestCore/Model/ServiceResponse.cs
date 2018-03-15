using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LUISApiTestCore.Model
{
    class ServiceResponse
    {
		public HttpStatusCode StatusCode { get; set; }
		public string Id { get; set; }		
		public bool IsSuccess { get; set; }
		public string message { get; set; }
		public string Errorcode { get; set; }
		public string ResponseMessage { get; set; }
	}

	class IntentServiceResponse
	{
		public ServiceResponse serviceresponse { get; set; }
		public string Intent { get; set; }
		public bool IsSuccess { get; set; }
	}

	
}
