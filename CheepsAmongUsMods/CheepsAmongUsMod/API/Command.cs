using CheepsAmongUsApi.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerControl = FFGALNAPKCD;

namespace CheepsAmongUsMod.API
{
    public class Command
    {
        public static List<Command> RegisteredCommands = new List<Command>();

        public string Cmd { get; }

        public string Syntax { get; set; }

        public string Description { get; set; }

        public Command(string command, string syntax = "", string desc = "")
        {
            Cmd = command;
            Syntax = syntax;
            Description = desc;

            RegisteredCommands.Add(this);
        }

        public virtual void Executed(string[] args) { }

        public void Unregister()
        {
            if(RegisteredCommands.Contains(this))
                RegisteredCommands.Remove(this);
        }
    }

    public class CommandManager
    {
        #region -------------------- Patch Rpc Send Chat -------------------- 
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
        public static class PatchRpcSendChat
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] string msg)
            {
                var args = msg.Split(' ');

                var maybeCmd = Command.RegisteredCommands.Where(x => x.Cmd.ToLower() == args[0].ToLower());

                if(maybeCmd.Count() >= 0)
                {
                    foreach (var cmd in maybeCmd)
                        cmd.Executed(args);

                    PlayerHudManager.HudManager.Chat.TextArea.SetText(string.Empty);  // Clear message box

                    return false;
                }

                return true;
            }
        }
        #endregion
    }
}
