using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CheepsAmongUsBaseMod;
using UnityEngine;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsBaseMod.ClientCommands;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;

namespace TeleportationGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class Teleportation : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusteleportation";
        public const string PluginName = "TeleportationGameMode";
        public const string PluginVersion = "1.1.5";

        public const string GameModeName = "Teleportation";

        public static int TeleportationDelay = 60;
        public const string Delimiter = "\n----------\n";

        private static int LastTeleported = 0;

        public static ManualLogSource _logger = null;

        Dictionary<MapType, Vector2[]> MapLocations = new Dictionary<MapType, Vector2[]>();

        private static System.Random RandomGen = new System.Random();

        public static bool IsThisGameModeActive
        {
            get
            {
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.CurrentGame == null)
                    return false;
                else
                    return CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.CurrentGame.GameState == GameStateEnum.Started &&
                        GameModeName == CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode;
            }
        }

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.InstalledGameModes.Add(GameModeName);

            #region ---------- Add Teleportation Locations ----------
            MapLocations.Add(MapType.Polus,
                new Vector2[]
                {
                    new Vector2(3, -12),
                    new Vector2(9, -12),
                    new Vector2(2, -17),
                    new Vector2(2, -24),
                    new Vector2(12, -23),
                    new Vector2(12, -16),
                    new Vector2(24, -22.5f),
                    new Vector2(20, -17.5f),
                    new Vector2(20, -11),
                    new Vector2(35, -8),
                    new Vector2(40, -10),
                    new Vector2(37, -21),
                    new Vector2(17, -3)
                });

            MapLocations.Add(MapType.Skeld,
                new Vector2[]
                {
                    new Vector2(-1, -1),
                    new Vector2(-8.8f, -3),
                    new Vector2(-13.5f, -4.5f),
                    new Vector2(-20.5f, -6),
                    new Vector2(-17.5f, 2.5f),
                    new Vector2(-2, -15.5f),
                    new Vector2(9.4f, 2.8f),
                    new Vector2(9.4f, -12),
                    new Vector2(4, -15.5f),
                    new Vector2(-7.5f, -8.5f),
                    new Vector2(-15, -12),
                    new Vector2(6.5f, -3.5f),
                    new Vector2(16.5f, -5)
                });

            MapLocations.Add(MapType.MiraHQ,
                new Vector2[] {
                    new Vector2(-4, 2.5f),
                    new Vector2(15, 0),
                    new Vector2(6, 1.5f),
                    new Vector2(6, 6),
                    new Vector2(2.4f, 11.5f),
                    new Vector2(8.5f, 13),
                    new Vector2(15, 4),
                    new Vector2(20, 4.5f),
                    new Vector2(23.8f, -1.7f),
                    new Vector2(25.5f, 3),
                    new Vector2(17.7f, 11.5f),
                    new Vector2(21, 20.6f),
                    new Vector2(14.8f, 20.3f),
                    new Vector2(17.8f, 23.8f)
                });
            #endregion

            #region ---------- Manage Commands ----------
            HelpCommand.AvailableCommands.Add("/tpdelay <Int>", $"Set the teleportation delay ({GameModeName})");

            ClientCommandManager.CommandExecuted += ClientCommandManager_CommandExecuted;
            Patching.ServerCommandExecuted += Patching_ServerCommandExecuted;
            #endregion

            #region ---------- Transmit Teleportation Delay on Start ----------
            GameStartedEvent.Listener += () =>
            {
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode != GameModeName || !CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                    return;

                Task.Run(async () =>
                {
                    await Task.Delay(2500);
                    CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("UpdateTeleportationDelay", $"{TeleportationDelay}");
                });
            };
            #endregion

            #region ---------- Teleportation Mode ----------
            HudUpdateEvent.Listener += () =>
            {
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode == GameModeName)
                    CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.GameSettingsAddition = $"\nTeleportation Delay: {Functions.ColorPurple}{TeleportationDelay}s[]";

                if (!IsThisGameModeActive || LastTeleported == 0)
                {
                    LastTeleported = Functions.GetUnixTime();
                    return;
                }

                string toDisplay = "----- [7a31f7ff]Teleportation[] -----\n";

                toDisplay += $"Teleportation in [11c5edff]{ TeleportationDelay - (Functions.GetUnixTime() - LastTeleported) }s[]";

                if (TeleportationDelay - (Functions.GetUnixTime() - LastTeleported) <= 0 && LastTeleported != 0)
                {
                    LastTeleported = Functions.GetUnixTime();
                    
                    if (MapLocations.ContainsKey(GameOptions.Map) && CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                    {
                        CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("TeleportNow", $"{true}");
                    }
                }

                string curText = PlayerHudManager.TaskText;

                if (!curText.Contains(Delimiter))
                {
                    PlayerHudManager.TaskText = toDisplay + Delimiter + PlayerHudManager.TaskText;
                }
                else if (!curText.Contains(toDisplay))
                {
                    string toReplace = curText.Split(new string[] { Delimiter }, StringSplitOptions.None)[0];

                    PlayerHudManager.TaskText = curText.Replace(toReplace, toDisplay);
                }
            };
            #endregion
        }

        private void Patching_ServerCommandExecuted(object sender, ServerCommandEventArgs e)
        {
            if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode != GameModeName)
                return;

            if (e.Command == "UpdateTeleportationDelay")
            {
                TeleportationDelay = int.Parse(e.Value);
            }

            if (e.Command == "TeleportNow")
            {
                LastTeleported = Functions.GetUnixTime();

                if (PlayerController.GetLocalPlayer().PlayerControl.inVent || PlayerController.GetLocalPlayer().PlayerData.IsDead) // Dont teleport if player is dead or in a vent
                    return;

                Vector2[] positions = MapLocations[GameOptions.Map];

                Vector2 toTp = positions[RandomGen.Next(0, positions.Length)];

                var ctrl = PlayerController.GetLocalPlayer();

                ctrl.RpcSnapTo(toTp);
            }
        }

        private void ClientCommandManager_CommandExecuted(object sender, CommandEventArgs e)
        {
            if (e.Command.ToLower().Equals("/tpdelay"))
            {
                e.Handled = true;

                if(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                    try
                    {
                        int delay = int.Parse(e.Arguments[1]);

                        TeleportationDelay = delay;

                        CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("UpdateTeleportationDelay", $"{TeleportationDelay}");

                        PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                            $"{Functions.ColorLime}The teleportation interval has been updated to {Functions.ColorPurple}{TeleportationDelay}s"
                            ); //Send syntax to player
                    }
                    catch
                    {
                        PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                            $"{Functions.ColorRed}Syntax[]: {Functions.ColorPurple}/tpdelay <Int>"
                            ); //Send syntax to player
                    }
                else
                    PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                        $"{Functions.ColorRed}Sorry, but only {Functions.ColorCyan}{CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.GetDecidingPlayer().PlayerData.PlayerName} " +
                        $"{Functions.ColorRed}can change the teleportation interval."
                        ); //Send syntax to player
            }
        }
    }
}
