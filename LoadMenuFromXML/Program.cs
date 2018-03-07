using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LoadMenuFromXML
{
	class Program
	{
		public static MenuXML menuXML = new MenuXML();
		protected const string MenusConfigFilename = "Menu.xml";
		private static double _menuCacheMinutes = 10;
		private static DateTime _menuNewDataRefreshTimestamp;

		static void Main(string[] args)
		{
			Console.WriteLine("Hello Menu!");
			var menu  = GetMenuItems().GetAwaiter().GetResult();
			ShowMenu(menu);
		}

		private static void ShowMenu(MenuXML menu)
		{
			List<string> MainMenu = new List<string>();

			if (menu != null)
			{
				foreach (var menu in menu.MainMenu)
				{
					
					
				}
				
			}
		}

		public static async Task<MenuXML> GetMenuItems()
		{			
			try
			{
				if (DateTime.Now > _menuNewDataRefreshTimestamp || menuXML.MainMenu == null)
				{
					var configFilePath = MenusConfigFilename; //Path.Combine(HostingEnvironment.ApplicationPhysicalPath, MenusConfigFilename);
					using (FileStream fileStream = new FileStream(configFilePath, FileMode.Open))
					{
						XmlSerializer serializer = new XmlSerializer(typeof(MenuXML));
						menuXML = (MenuXML)serializer.Deserialize(fileStream);
						_menuNewDataRefreshTimestamp = DateTime.Now.AddMinutes(_menuCacheMinutes);
						System.Diagnostics.Trace.TraceInformation("Menu caching triggered triggered at: " + DateTime.Now.ToString() + ". Next trigger will happen at: " + _menuNewDataRefreshTimestamp);
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.TraceInformation($"Something went wrong while deserializing the Menu XML. Error message: {ex.Message}. Error stack: {ex.StackTrace}");
			}
			return menuXML;
		}
	}
}
