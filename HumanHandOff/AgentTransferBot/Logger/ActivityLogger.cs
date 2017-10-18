using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.History;
using Microsoft.Bot.Connector;
using System.Collections.Generic;

namespace AgentTransferBot
{
    public class ActivityLogger : IActivityLogger
    {
        public static List<string> ChatContainer = new List<string>();
        

        
        public async Task LogAsync(IActivity activity)
        {
            Debug.WriteLine($"From:{activity.From.Id} - To:{activity.Recipient.Id} - Message:{activity.AsMessageActivity()?.Text}");
            var message = $"**{activity.From.Name}**: {activity.AsMessageActivity()?.Text}";
            ChatContainer.Add(message);
        }

        
    }
}