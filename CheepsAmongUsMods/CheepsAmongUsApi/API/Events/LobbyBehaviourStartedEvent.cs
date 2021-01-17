using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsApi.API.Events
{
    public class LobbyBehaviourStartedEvent
    {
        /// <summary>
        /// Listener for the lobby behaviour started event
        /// </summary>
        public static event Notify Listener;

        public static void CallEvent()
        {
            Listener?.Invoke();
        }
    }
}
