using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentTransferBot
{
    /// <summary>
    /// This interface implementation is not consumed in the solution. The idea behind this implementation was to show the agent the list of avaiaible users
    /// </summary>
    public class InMemoryUserStore : IUserProvider
    {
        private ConcurrentDictionary<string, string> _availableUsers = new ConcurrentDictionary<string, string>();
        private static object objectLock = new object();
        public bool AddUser(User user)
        {
            try
            {
                _availableUsers.AddOrUpdate(user.ConversationReference.User.Id, user.ConversationReference.User.Name,(k,v) => user.ConversationReference.User.Name);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string RemoveUser(User user)
        {
            lock (objectLock)
            {
                string res;
                _availableUsers.TryRemove(user.ConversationReference.User.Id, out res);
                return res;
            }
        }

        public List<string> RetrieveUserList()
        {
           return _availableUsers.Values.ToList();
        }
    }
}