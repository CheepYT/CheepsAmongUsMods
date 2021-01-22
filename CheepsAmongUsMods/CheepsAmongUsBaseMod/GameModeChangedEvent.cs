using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsBaseMod
{
    public class GameModeChangedEvent
    {
        public static event EventHandler<GameModeChangeEventArgs> Listener;

        internal static void ExecuteGameModeChangedEvent(string previous, string current)
        {
            Listener?.Invoke(null, new GameModeChangeEventArgs() { PreviousGameMode = previous, CurrentGameMode = current }) ;
        }
    }

    public class GameModeChangeEventArgs : EventArgs
    {
        public string PreviousGameMode { get; set; }
        public string CurrentGameMode { get; set; }
    }
}
