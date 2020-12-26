using CheepsAmongUsApi.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PlayerControl = FFGALNAPKCD;

namespace CheepsAmongUsBaseMod.ClientCommands
{
    public class ClientCommandManager
    {
        public const string CommandPrefix = "/";

        #region ----- Command Event -----
        public static event EventHandler<CommandEventArgs> CommandExecuted;
        protected static void OnExecutedCommand(CommandEventArgs e)
        {
            CommandExecuted?.Invoke(null, e);
        }
        #endregion

        public static void Start()
        {
            HelpCommand.Start();
            GameModeCommand.Start();
        }

        private static bool ManageCommand(string cmd)
        {
            bool managed =
                HelpCommand.ManageCommand(cmd) ||
                GameModeCommand.ManageCommand(cmd);

            #region ----- Call Command Event -----
            var eventArgs = new CommandEventArgs();
            eventArgs.Arguments = cmd.Split(' ');
            eventArgs.Command = eventArgs.Arguments[0];

            try
            {
                OnExecutedCommand(eventArgs);
            } catch (Exception e)
            {
                CheepsAmongUsBaseMod._logger.LogError("Error occured while attempting to invoke CommandExecuted: " + e);
            }

            managed = managed || eventArgs.Handled;
            #endregion

            if (managed)
                PlayerHudManager.HudManager.Chat.TextArea.SetText(string.Empty);  // If the command has been handled, clear message box

            return managed;
        }

        #region -------------------- Patch Rpc Send Chat -------------------- 
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
        public static class PatchRpcSendChat
        {
            public static bool Prefix(PlayerControl __instance, string PGIBDIEPGIC)
            {
                if (!new PlayerController(__instance).AmPlayerController() || !PGIBDIEPGIC.StartsWith(CommandPrefix))
                    return true;

                return !ManageCommand(PGIBDIEPGIC);
            }
        }
        #endregion
    }

    public class CommandEventArgs : EventArgs
    {
        public string Command { get; set; }
        public bool Handled { get; set; }
        public string[] Arguments { get; set; }
    }
}
