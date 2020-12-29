using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using CheepsAmongUsBaseMod;
using CheepsAmongUsBaseMod.ClientCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;

namespace QuantumTunnelingGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class QuantumTunneling : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusquantumtunneling";
        public const string PluginName = "QuantumTunnelingGameMode";
        public const string PluginVersion = "1.0.0";

        public const string GameModeName = "QuantumTunneling";

        public const string Delimiter = "\n----------\n";

        private static bool IsEnabled;
        private static int LastEnabled;

        public const KeyCode UseKey = KeyCode.C;

        public static int OnTime = 30;
        public static int UseCooldown = 20;

        public static ManualLogSource _logger = null;

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

            HelpCommand.AvailableCommands.Add("/qtgm <OnTime | Cooldown> <Int>", $"Set gamemode properties ({GameModeName})");

            ClientCommandManager.CommandExecuted += ClientCommandManager_CommandExecuted;
            Patching.ServerCommandExecuted += Patching_ServerCommandExecuted;

            #region ---------- Transmit Properties on Start ----------
            GameStartedEvent.Listener += () =>
            {
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode != GameModeName || !CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                    return;

                Task.Run(async () =>
                {
                    await Task.Delay(2500);
                    CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("UpdateOnTime", $"{OnTime}");
                    CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("UpdateUseCooldown", $"{UseCooldown}");
                });
            };
            #endregion

            HudUpdateEvent.Listener += () =>
            {
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode == GameModeName)
                    CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.GameSettingsAddition =
                    $"\nAbility Active Period: {Functions.ColorPurple}{OnTime}s[]\n" +
                    $"Ability Cooldown: {Functions.ColorPurple}{UseCooldown}s[]";

                if (!IsThisGameModeActive || !PlayerController.GetLocalPlayer().PlayerData.IsImpostor)
                    return;

                string toDisplay = "--- [7a31f7ff]Quantum Tunneling []---\n";

                if (IsEnabled)
                {
                    toDisplay += $"Collision in [11c5edff]{ OnTime - (Functions.GetUnixTime() - LastEnabled) }s[]";

                    if (OnTime - (Functions.GetUnixTime() - LastEnabled) <= 0)
                    {
                        PlayerController ctrl = PlayerController.GetLocalPlayer();
                        IsEnabled = false;

                        CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand($"ImpostorDematCmd", $"{ ctrl.PlayerData.PlayerName };{ IsEnabled }");
                    }
                }
                else
                {
                    bool CanEnable = LastEnabled + OnTime + UseCooldown <= Functions.GetUnixTime();

                    if (CanEnable)
                    {
                        // ----- Can enable demat -----
                        if (Functions.GetKey(UseKey) && PlayerController.GetLocalPlayer().Moveable)
                        {
                            // ----- User pressed key -----
                            LastEnabled = Functions.GetUnixTime();

                            PlayerController ctrl = PlayerController.GetLocalPlayer();
                            IsEnabled = true;

                            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand($"ImpostorDematCmd", $"{ ctrl.PlayerData.PlayerName };{ IsEnabled }");
                        }
                        else
                        {
                            // ----- Display can press key -----
                            toDisplay += $"Use [11c5edff]{ UseKey }[] to activate.";
                        }
                    }
                    else
                    {
                        // ----- Cannot enable demat -----
                        toDisplay += $"Cooldown: [11c5edff]{ (UseCooldown + OnTime) - (Functions.GetUnixTime() - LastEnabled) }s[]";
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
        }

        private void Patching_ServerCommandExecuted(object sender, ServerCommandEventArgs e)
        {
            if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode != GameModeName)
                return;

            if (e.Command == "ImpostorDematCmd")
            {
                string plName = e.Value.Split(';')[0];
                bool enabled = bool.Parse(e.Value.Split(';')[1]);

                PlayerController ctrl = PlayerController.FromName(plName);
                ctrl.HasCollision = !enabled;

                if (ctrl.PlayerControl.CurrentPet.sadClip != null)
                    ctrl.PlayerControl.CurrentPet.Collider.enabled = !enabled;

                if (PlayerController.GetLocalPlayer().PlayerData.IsImpostor || PlayerController.GetLocalPlayer().PlayerData.IsDead)
                    ctrl.SetOpacity(enabled ? 0.3f : 1f);
            } else if(e.Command == "UpdateOnTime")
            {
                OnTime = int.Parse(e.Value);
            } else if(e.Command == "UpdateUseCooldown")
            {
                UseCooldown = int.Parse(e.Value);
            }
        }

        private void ClientCommandManager_CommandExecuted(object sender, CommandEventArgs e)
        {
            if (e.Command.ToLower().Equals("/qtgm"))
            {
                e.Handled = true;

                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                {
                    var cmd = e.Arguments[1].ToLower();

                    if (cmd.Equals("ontime"))
                    {
                        try
                        {
                            int delay = int.Parse(e.Arguments[2]);

                            OnTime = delay;

                            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("UpdateOnTime", $"{OnTime}");

                            PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                                $"{Functions.ColorLime}The active period has been updated to {Functions.ColorPurple}{OnTime}s"
                                ); //Send syntax to player
                        }
                        catch
                        {
                            PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                                $"{Functions.ColorRed}Syntax[]: {Functions.ColorPurple}/qtgm <OnTime> <Int>"
                                ); //Send syntax to player
                        }
                    } else if (cmd.Equals("cooldown"))
                    {
                        try
                        {
                            int delay = int.Parse(e.Arguments[2]);

                            UseCooldown = delay;

                            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("UpdateUseCooldown", $"{UseCooldown}");

                            PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                                $"{Functions.ColorLime}The cooldown has been updated to {Functions.ColorPurple}{UseCooldown}s"
                                ); //Send syntax to player
                        }
                        catch
                        {
                            PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                                $"{Functions.ColorRed}Syntax[]: {Functions.ColorPurple}/qtgm <Cooldown> <Int>"
                                ); //Send syntax to player
                        }
                    } else
                    {
                        PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                            $"{Functions.ColorRed}Syntax[]: {Functions.ColorPurple}/qtgm <OnTime | Cooldown> <Int>"
                            ); //Send syntax to player
                    }
                } 
                else
                    PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                        $"{Functions.ColorRed}Sorry, but only {Functions.ColorCyan}{CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.GetDecidingPlayer().PlayerData.PlayerName} " +
                        $"{Functions.ColorRed}can change the gamemode properties."
                        ); //Send syntax to player
            }
        }
    }
}
