using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Multipurpose
{
    [Serializable]
    [LuisModel("85aaccef-4c54-4318-80bc-912123a03d5a", "8f6d88e12e98423e8e74401ace73c4f4")]
    public class RootLuisDialog : LuisDialog<object>
    {
        
        [LuisIntent("GetChannelUrl")]
        public async Task AgentTransfer(IDialogContext context, IAwaitable<IMessageActivity> iMessage, LuisResult luisResult)
        {
            var message = "Welcome, please click one of the below links to open the Bot in your desired channel";
            await context.PostAsync(message);

            message = "**[Teams Channel]( " + ChannelUrl.GetTeamsURL() + ")**\n\n";
            message += "**[Skype Channel]( " + ChannelUrl.GetSkypeURL() + ")**\n\n";
            message += "**[WebChat Channel](" + ChannelUrl.GetWebChatURL() + ")**\n\n";

            await context.PostAsync(message);
            context.Done<object>(null);
        }

        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, IAwaitable<IMessageActivity> iMessage, LuisResult luisResult)
        {
            var message = "Welcome, I'm multipurpose bot.";
            await context.PostAsync(message);
            context.Done<object>(null);
        }

        [LuisIntent("None")]
        [LuisIntent("")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> iMessage, LuisResult luisResult)
        {
            var message = "Hooray!! I don't know what to reply. Did you forget to train me? or missing an intent?";
            await context.PostAsync(message);          
            context.Done<object>(null);
        }
    }
}
