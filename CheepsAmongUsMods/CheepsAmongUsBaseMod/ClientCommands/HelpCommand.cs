using CheepsAmongUsApi.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsBaseMod.ClientCommands
{
    public class HelpCommand
    {
        public const string Command = "/help";
        public const string AvailableCommandsColor = "[349eebff]";
        public const string CommandColor = "[be09d6ff]";

        public static Dictionary<string, string> AvailableCommands = new Dictionary<string, string>();

        public static void Start()
        {
            AvailableCommands.Add("/help", "Displays all available commands");
        }

        public static bool ManageCommand(string cmd)
        {
            // Split the message into arguments
            string[] args = cmd.ToLower().Split(' ');

            if (args[0].Equals(Command.ToLower()))
            {
                string commands = "";

                foreach (var keyVal in AvailableCommands)
                    commands += $"{CommandColor}{keyVal.Key}[].....{keyVal.Value}\n";

                PlayerHudManager.AddChat(PlayerController.LocalPlayer,
                    $"-------------------- {AvailableCommandsColor}Available Commands []--------------------\n" +
                    commands +
                    $"-------------------- {AvailableCommandsColor}Available Commands []--------------------"
                    ); //Send chat to player

                return true; //Return true, to tell command manager, that this message has been handled
            }

            return false;
        }
    }
}
