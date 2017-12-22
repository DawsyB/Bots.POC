using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Bot.Multipurpose.Forms;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Bot.Multipurpose.Dialogs
{
	[Serializable]
	public class FeedbackDialog : IDialog<object>
	{
		public async Task StartAsync(IDialogContext context)
		{
			var message = context.Activity as IMessageActivity;
			var query = FeedbackForm.Parse(message.Value);

			await context.PostAsync("Looks all good. Program me to send the feedback email");
		}
	}
}