using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace SPOnlineCSOM
{
	class Program
	{
		static void Main(string[] args)
		{
			//SP_kb_kb_1
			string clienturl = "https://cisdev01.sharepoint.com/";
			//https://cisdev01.sharepoint.com/Lists/LUIS%20Site%20List%20manager/

			try
			{
				using (ClientContext context = new ClientContext(clienturl))
				{
					string username = "daws@cisdev01.onmicrosoft.com";
					string pass = "P@ssword!23";
					SecureString passWord = new SecureString();
					foreach (char c in pass.ToCharArray()) passWord.AppendChar(c);
					context.Credentials = new SharePointOnlineCredentials(username, passWord);


					List list = context.Web.GetList("https://cisdev01.sharepoint.com/Lists/LUIS%20Site%20List%20manager/");
					
					CamlQuery query = CamlQuery.CreateAllItemsQuery(100);
					ListItemCollection items = list.GetItems(query);
					context.Load(items);
					context.ExecuteQuery();

					foreach (ListItem listItem in items)
					{
						// We have all the list item data. For example, Title. 
						var test = listItem["Title"];
					}
				}
			}
			catch (Exception ex)
			{

			}

		}
	}
}
