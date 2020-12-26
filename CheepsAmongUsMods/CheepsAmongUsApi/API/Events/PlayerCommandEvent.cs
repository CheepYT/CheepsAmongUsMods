﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsApi.API.Events
{
    public class PlayerCommandEvent
    {
        /// <summary>
        /// Listener for the player chat event
        /// </summary>
        public static event Notify Listener;

        public static void CallEvent()
        {
            Listener?.Invoke();
        }
    }
}
