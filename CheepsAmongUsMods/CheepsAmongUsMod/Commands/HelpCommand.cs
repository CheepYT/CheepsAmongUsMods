using CheepsAmongUsApi.API;
using CheepsAmongUsMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsMod.Commands
{
    public class HelpCommand : Command
    {
        public const string CommandStr = "/help";
        public const string AvailableCommandsColor = "[349eebff]";
        public const string CommandColor = "[be09d6ff]";

        public HelpCommand() : base(CommandStr, CommandStr, "Displays all available commands") { }

        public override void Executed(string[] args)
        {
            string commands = "";

            foreach (var cmd in RegisteredCommands)
                if(cmd.Syntax != "" && cmd.Description != "")
                    commands += $"{CommandColor}{cmd.Syntax}[].....{cmd.Description}\n";

            PlayerHudManager.AddChat(PlayerController.LocalPlayer,
                $"-------------------- {AvailableCommandsColor}Available Commands []--------------------\n" +
                commands +
                $"-------------------- {AvailableCommandsColor}Available Commands []--------------------"
                ); //Send chat to player

            base.Executed(args);
        }
    }
}
