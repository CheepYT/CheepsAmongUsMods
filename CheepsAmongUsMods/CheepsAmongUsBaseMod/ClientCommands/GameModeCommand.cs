using CheepsAmongUsApi.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsBaseMod.ClientCommands
{
    public class GameModeCommand
    {
        public const string Command = "/gamemode";

        public static void Start()
        {
            HelpCommand.AvailableCommands.Add("/gamemode <Mode>", "Changes the GameMode");
        }

        public static bool ManageCommand(string cmd)
        {
            // Split the message into arguments
            string[] args = cmd.ToLower().Split(' ');

            if (args[0].Equals(Command.ToLower()))
            {
                if (args.Length != 2)
                {
                    // If a gamemode is not given

                    string modes = "";

                    foreach (string val in CheepsAmongUsBaseMod.InstalledGameModes)
                        modes += val + " | ";

                    modes = modes.Substring(0, modes.Length - 3);

                    PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                        $"{Functions.ColorRed}Syntax[]: {Functions.ColorPurple}{Command} <{modes}>"
                        ); //Send syntax to player
                }
                else
                {
                    string cmode = "N/A";

                    foreach (var mode in CheepsAmongUsBaseMod.InstalledGameModes)
                        if (args[1].ToLower() == mode.ToLower())
                        {
                            cmode = mode;
                            break;
                        }

                    if (cmode == "N/A")
                    {
                        string modes = "";

                        foreach (string val in CheepsAmongUsBaseMod.InstalledGameModes)
                            modes += val + " | ";

                        modes = modes.Substring(0, modes.Length - 3);

                        PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                            $"{Functions.ColorRed}Syntax[]: {Functions.ColorPurple}{Command} <{modes}>"
                            ); //Send syntax to player
                    }
                    else
                    {
                        CheepsAmongUsBaseMod.ActiveGameMode = cmode; // Change gamemode

                        CheepsAmongUsBaseMod.SendModCommand("GameMode", cmode); // Broadcast change

                        PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                            $"{Functions.ColorLime}The GameMode has been changed to {Functions.ColorPurple}{cmode}{Functions.ColorLime}."
                            ); //Send confirmation to player
                    }
                }

                return true; //Return true, to tell command manager, that this message has been handled
            }

            return false;
        }
    }
}
