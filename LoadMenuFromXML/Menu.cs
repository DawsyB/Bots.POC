using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace LoadMenuFromXML
{

	[Serializable]
	[XmlRoot("menulist")]
	public class MenuXML
	{
		[XmlElement("menutitle")]
		public string MenuTitle { get; set; }

		[XmlElement("topmenu")]
		public List<MainMenu> MainMenu { get; set; }


	}

	[Serializable]
	public class MainMenu
	{
		[XmlElement("menuitem")]
		public string MenuItem { get; set; }

		[XmlElement("menutitle")]
		public string MenuTitle { get; set; }

		[XmlElement("submenu")]
		public List<SubMenu> _SubMenu { get; set; }
	}

	[Serializable]
	public class SubMenu
	{
		[XmlElement("menuitem")]
		public string MenuItem { get; set; }

		[XmlElement("menutitle")]
		public string MenuTitle { get; set; }

		[XmlElement("submenu")]
		public List<SubMenu> _SubMenu { get; set; }
	}
}
