using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsApi.API.Events
{
    public class GameEndedEvent
    {
        /// <summary>
        /// Listener for the game ended event
        /// </summary>
        public static event Notify Listener;

        public static void CallEvent()
        {
            Listener?.Invoke();
        }
    }
}
