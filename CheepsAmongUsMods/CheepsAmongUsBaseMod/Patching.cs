using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameObjectClass = KHNHJFFECBP;
using PlayerControlClass = FFGALNAPKCD;
using ChatClass = GEHKHGLKFHE;
using StatsManager = ENHLBAECCDF;

using CheepsAmongUsApi.API;

namespace CheepsAmongUsBaseMod
{
    public class Patching
    {
        public static event EventHandler<ServerCommandEventArgs> ServerCommandExecuted;

        #region -------------------- Patch Chat Event -------------------- 
        [HarmonyPatch(typeof(ChatClass), nameof(ChatClass.AddChat))]
        public static class Patch_ChatClass_ExitGame
        {
            public static bool Prefix(PlayerControlClass KMCAKLLFNIM, ref string PGIBDIEPGIC)
            {
                try
                {
                    if (PGIBDIEPGIC.StartsWith($"MessageFrom{CheepsAmongUsBaseMod.PluginName}:"))
                    {
                        var message = PGIBDIEPGIC.Split(':')[1].Split('=');
                        var cmd = message[0];
                        var val = message[1];

                        switch (cmd)
                        {
                            case "GameMode":
                                {
                                    if(CheepsAmongUsBaseMod.ActiveGameMode != val)
                                        GameModeChangedEvent.ExecuteGameModeChangedEvent(CheepsAmongUsBaseMod.ActiveGameMode, val);

                                    CheepsAmongUsBaseMod.GameSettingsAddition = string.Empty; // Reset Game Settings Addition

                                    CheepsAmongUsBaseMod.ActiveGameMode = val;
                                    PlayerHudManager.AppendedPingText = $"\nMode: {Functions.ColorPurple}{ CheepsAmongUsBaseMod.AddSpaces(val) }[]\n" +
                                        $">> {Functions.ColorGreen}Cheep-YT.com[] <<";

                                    if (!CheepsAmongUsBaseMod.InstalledGameModes.Contains(val))
                                    {
                                        PlayerHudManager.HudManager.Chat.ForceClosed();

                                        Functions.ShowPopUp(
                                            $"[42ecf5ff]----- [7a31f7ff]{ CheepsAmongUsBaseMod.AddSpaces(val) } [42ecf5ff]-----[]\n" +
                                            $"You currently don't have this GameMode installed! Please install it."
                                            );
                                    }

                                    break;
                                }

                            default:
                                {
                                    ServerCommandEventArgs args = new ServerCommandEventArgs
                                    {
                                        Command = cmd,
                                        Value = val
                                    };

                                    try
                                    {
                                        OnExecutedCommand(args);
                                    }
                                    catch (Exception e)
                                    {
                                        CheepsAmongUsBaseMod._logger.LogError("Error occured while attempting to invoke ServerCommandExecuted: " + e);
                                    }

                                    break;
                                }
                        }

                        return false; // Return false, to cancel recv
                    }
                }
                catch { }

                return true; // Return true, to not cancel
            }
        }


        #region ----- Command Event -----
        protected static void OnExecutedCommand(ServerCommandEventArgs e)
        {
            ServerCommandExecuted?.Invoke(null, e);
        }
        #endregion

        #endregion

        #region -------------------- Game Connect Event --------------------
        [HarmonyPatch(typeof(GameObjectClass), nameof(GameObjectClass.Connect))]
        public static class Patch_GameObjectClass
        {
            public static void Postfix(GameObjectClass __instance)
            {
                CheepsAmongUsBaseMod.CurrentGame = __instance;
            }
        }
        #endregion
    }

    public class ServerCommandEventArgs : EventArgs
    {
        public string Command { get; set; }
        public string Value { get; set; }
    }
}
