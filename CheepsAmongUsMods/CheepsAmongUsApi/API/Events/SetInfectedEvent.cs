using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsApi.API.Events
{
    public class SetInfectedEvent
    {
        /// <summary>
        /// Listener for the set infected event
        /// Only executed by the host player
        /// </summary>
        public static event Notify Listener;

        public static void CallEvent()
        {
            Listener?.Invoke();
        }
    }
}
