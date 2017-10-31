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

namespace AgentTransferBot
{
    [Serializable]
    [LuisModel("c2e62aa8-51b8-4ab7-8fe1-c3c52590f93c", "8f6d88e12e98423e8e74401ace73c4f4")]
    public class TransferLuisDialog : LuisDialog<object>
    {
        private readonly IUserToAgent _userToAgent;

        public TransferLuisDialog(IUserToAgent userToAgent)
        {
            _userToAgent = userToAgent;
        }
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult luisResult)
        {
            await context.PostAsync("I didn't understand you.");
            await context.PostAsync("You can contact our customer care representative by typing \"Connect me with customer care\"");
            context.Done<object>(null);
        }

        [LuisIntent("Greeting")]
        public async Task greeting(IDialogContext context, LuisResult luisResult)
        {
            await context.PostAsync("Hey there");
            await context.PostAsync("How can I help you today?");
            context.Done<object>(null);
        }


        [LuisIntent("AgentTransfer")]
        public async Task AgentTransfer(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var activity = await message;
            var agent = await _userToAgent.IntitiateConversationWithAgentAsync(activity as Activity, default(CancellationToken));
            if (agent == null)
                await context.PostAsync("All our customer care representatives are busy at the moment. Please try after some time.");
            context.Done<object>(null);
        }
    }
}
