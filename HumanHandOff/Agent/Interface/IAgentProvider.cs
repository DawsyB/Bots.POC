using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentTransferBot
{
    public interface IAgentProvider
    {
        /// <summary>
        /// Gets the next available agent. Also removes the agent from availaibilty pool
        /// </summary>
        /// <returns></returns>
        Agent GetNextAvailableAgent();
        /// <summary>
        /// Adds the agent to the availaibility pool as soon as the agent connects
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        bool AddAgent(Agent agent);

        /// <summary>
        /// Removes the agent from the availability pool as soon as the agent disconnects
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        Agent RemoveAgent(Agent agent);
    }
}
