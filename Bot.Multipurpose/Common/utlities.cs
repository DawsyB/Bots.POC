using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace Bot.Multipurpose
{
    public class utlities
    {
		public static IMessageActivity BuildPromptChoice(IMessageActivity resultMessage, List<string> confirmOptionsSet, string promptTitle)
		{

			resultMessage.Attachments = new List<Attachment>();

			Attachment attachment = new Attachment();

			attachment.ContentType = "application/vnd.microsoft.card.hero";
			HeroCard card = new HeroCard();
			card.Text = promptTitle;
			card.Buttons = new List<CardAction>();
			int counter = 1;
			foreach (var option in confirmOptionsSet)
			{
				CardAction cardaction = new CardAction();
				cardaction.Type = "imBack";
				cardaction.Title = option;
				cardaction.Value = option;
				card.Buttons.Add(cardaction);
				counter++;
			}
			attachment.Content = card;
			resultMessage.Attachments.Add(attachment);
			return resultMessage;
		}

		public static int GetRandomNumber(int start, int end)
		{
			var rand = new Random();
			return rand.Next(start, end);
		}

		/// <summary>
		/// Reads the JSON Card and returns as attachment
		/// </summary>
		/// <param name="JsonCardFile"></param>
		/// <returns></returns>
		public static Attachment GetAttachment(string JsonCardFile)
		{
			try
			{
				string configFilePath = JsonCardFile;
				configFilePath = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, JsonCardFile);
				var json = File.ReadAllText(configFilePath);
				Attachment attachment = new Attachment();
				attachment = JsonConvert.DeserializeObject<Attachment>(json);
				return attachment;
			}
			catch (Exception ex)
			{
				
				return null;
			}
		}
	}
}