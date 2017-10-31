using Bot.Multipurpose.Dialogs;
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
        public RootLuisDialog(ILuisService service)
            : base(service)
        {
        }
        public RootLuisDialog()
        { }

        public static int greetingcounter = 1;

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

        [LuisIntent("CardDesigns")]
        public async Task CardDesigns(IDialogContext context, IAwaitable<IMessageActivity> iMessage, LuisResult luisResult )
        {

            EntityRecommendation luisEntityType;
            if (luisResult.TryFindEntity(constants.CardDesigns.AdaptiveCard, out luisEntityType))
            {
                context.Call(new AdaptiveCardDialog(), this.ResumeFromDialogCallBack);
            }

        }

        [LuisIntent("SampleCards")]
        public async Task Cards(IDialogContext context, IAwaitable<IMessageActivity> iMessage, LuisResult luisResult)
        {
                      
            context.Call(new CardsDialog(), this.ResumeFromDialogCallBack);
            
        }

        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, IAwaitable<IMessageActivity> iMessage, LuisResult luisResult)
        {
            try
            {
                               
                var incomingMessage = string.Empty;
                if (greetingcounter == 1)
                {

                    incomingMessage = "Hey. I'm not trained enough to do anything!";
                    incomingMessage += "\n\n You said: " + (await iMessage).Text;
                    greetingcounter++;

                }
                else
                {
                    incomingMessage = "You said: " + (await iMessage).Text;
                }
                
                await context.PostAsync(incomingMessage);
            }
            catch (Exception ex)
            {
                await context.PostAsync("Found some error "+ex.Message);
            }
            context.Done<object>(null);
            }

        [LuisIntent("None")]
        [LuisIntent("")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> iMessage, LuisResult luisResult)
        {
            var message = "Hooray!! I don't know what to reply. My developer has not designed me to handle this intent?";
            await context.PostAsync(message);          
            context.Done<object>(null);
        }

        private async Task ResumeFromDialogCallBack(IDialogContext context, IAwaitable<object> result)
        {
            context.Done<object>(null);
        }
    }
}
