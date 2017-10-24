using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentTransferBot
{
    public interface IUserToAgent
    {
        /// <summary>
        /// Checks if routing is required
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AgentTransferRequiredAsync(Activity message, CancellationToken cancellationToken);

        /// <summary>
        /// Send messages from user to agent
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendToAgentAsync(Activity message, CancellationToken cancellationToken);

        /// <summary>
        /// Initiate a transfer for the first time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Agent> IntitiateConversationWithAgentAsync(Activity message, CancellationToken cancellationToken);
    }
}
