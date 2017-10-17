using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentTransferBot
{
    /// <summary>
    /// This interface is not consumed in the solution!
    /// </summary>
    public interface IUserProvider
    {
        
        /// <summary>
        /// Adds the user to the pool as soon as the user connects
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        bool AddUser(User agent);

        /// <summary>
        /// Removes the agent from the pool as soon as the user disconnects
        /// </summary>
        /// <param name="agent"></param>
        /// <returns></returns>
        string RemoveUser(User agent);

        List<string> RetrieveUserList();
    }
}