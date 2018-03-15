using LUISApiV2._0_Test.LUIS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LUISApiV2._0_Test
{
	class Program
	{
		static void Main(string[] args)
		{
			var result = CreateLuisApp.MakeRequest().GetAwaiter().GetResult();

			 
			//result.Content.ReadAsStringAsync().Result; -  to return the id
			Console.WriteLine("Hit ENTER to exit...");
			Console.ReadLine();
		}
	}
}
