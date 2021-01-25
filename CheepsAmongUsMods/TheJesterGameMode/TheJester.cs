using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using CheepsAmongUsBaseMod;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;
using ShipStatusClass = HLBNNHFCNAJ;
using GameEndReasonEnum = AIMMJPEOPEC;

namespace JesterGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class TheJester : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusjester";
        public const string PluginName = "JesterGameMode";
        public const string PluginVersion = "1.0.45";

        public const string GameModeName = "Jester";

        public const string Delimiter = "\n----------";

        internal static ManualLogSource _logger = null;

        public static bool IsThisGameModeSelected
        {
            get
            {
                return GameModeName == CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode;
            }
        }

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

        internal static RolePlayer JesterRolePlayer = null;
        internal static RolePlayer DefeatRole = null;
        internal static bool HasJesterWon = false;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.InstalledGameModes.Add(GameModeName);

            #region ---------- Enable Harmony Patching ----------
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            CheepsAmongUsBaseMod.Patching.ServerCommandExecuted += Patching_ServerCommandExecuted;

            var random = new System.Random();


            #region ---------- GameMode Started ----------
            GameStartedEvent.Listener += () =>
            {
                JesterRolePlayer = null;

                Task.Run(async () =>
                {
                    await Task.Delay(100);

                    while (PlayerController.AllPlayerControls.Count == 1)
                        await Task.Delay(1);

                    if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.IsDecidingClient)
                    {
                        var available = PlayerController.AllPlayerControls.Where(x => !x.PlayerData.IsImpostor).ToList();
                        var jester = available[random.Next(available.Count)];

                        CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("SetJester", $"{jester.PlayerId}");
                    }
                });
            };
            #endregion

            #region ---------- GameMode Running ----------
            HudUpdateEvent.Listener += () =>
            {
                try
                {
                    if (!IsThisGameModeActive || JesterRolePlayer == null)
                        return;

                    if(PlayerController.AllPlayerControls.Where(x => x.PlayerData.IsImpostor && !x.PlayerData.IsDead).Count() == 0 || ShipStatusClass.Instance.CheckTaskCompletion())
                        JesterRolePlayer.PlayerController.PlayerData.IsImpostor = true;

                    if (!JesterRolePlayer.AmRolePlayer)
                        return;

                    string toDisplay = "----- [7a31f7ff]Jester[] -----\n";
                    toDisplay += "You are [410dffff]The Jester[]\n" +
                                 "Trick the crewmates into thinking\n" +
                                 "that you are an [ff0000ff]Impostor[]\n" +
                                 "Your goal is to [ff0000ff]Get Exiled[]";

                    PlayerHudManager.TaskText = toDisplay + Delimiter;
                }
                catch { }
            };
            #endregion

            #region ---------- Game Ended ----------
            LobbyBehaviourStartedEvent.Listener += () =>
            {
                JesterRolePlayer = null;
                DefeatRole = null;
                HasJesterWon = false;
                ReceivedBy = 0;
                Patching.Patch_ShipStatusClass_RpcEndGame.CanEndGame = false;
            };
            #endregion
        }

        static int ReceivedBy = 0;

        private void Patching_ServerCommandExecuted(object sender, ServerCommandEventArgs e)
        {
            if (!IsThisGameModeSelected)
                return;

            if (e.Command == "SetJester")
            {
                #region ---------- Set the Jester ----------
                var playerId = byte.Parse(e.Value);
                var jester = PlayerController.FromPlayerId(playerId);

                JesterRolePlayer = new RolePlayer(jester, "Jester");

                jester.PlayerTaskObjects = new Il2CppSystem.Collections.Generic.List<PILBGHDHJLH>();
                jester.ClearTasks();

                var intro = JesterRolePlayer.RoleIntro;
                intro.UseRoleIntro = true;
                intro.RoleDescription =
                    $"Trick the crewmates into thinking\n" +
                    $"that you are an {Functions.ColorRed}Impostor[]\n";
                intro.RoleNameColor = new Color(175 / 255f, 43 / 255f, 237 / 255f);
                intro.BackgroundColor = new Color(127 / 255f, 0 / 255f, 186 / 255f);

                var eject = JesterRolePlayer.RoleEjected;
                eject.UseRoleEjected = true;    // Custom ejected

                if (jester.IsLocalPlayer)
                    jester.PlayerControl.nameText.Color = new Color(0.74901960784f, 0, 1f);
                else
                {
                    DefeatRole = new RolePlayer(PlayerController.LocalPlayer, "Other");
                    DefeatRole.RoleOutro.WinText = "Defeat";
                    DefeatRole.RoleOutro.WinTextColor = new Color(175 / 255f, 43 / 255f, 237 / 255f);
                    DefeatRole.RoleOutro.BackgroundColor = new Color(127 / 255f, 0 / 255f, 186 / 255f);
                }
                #endregion
            } else if(e.Command == "JesterWon" && !HasJesterWon)
            {
                HasJesterWon = true;

                #region ---------- Jester has won ----------
                var outro = JesterRolePlayer.RoleOutro;
                outro.UseRoleOutro = true;
                outro.WinText = "Victory";
                outro.WinTextColor = new Color(175 / 255f, 43 / 255f, 237 / 255f);
                outro.BackgroundColor = new Color(127 / 255f, 0 / 255f, 186 / 255f);

                if (!JesterRolePlayer.AmRolePlayer)
                    DefeatRole.RoleOutro.UseRoleOutro = true;

                foreach (var player in PlayerController.AllPlayerControls.Where(x => x.PlayerId != TheJester.JesterRolePlayer.PlayerController.PlayerId))
                    player.PlayerData.IsImpostor = false;

                JesterRolePlayer.PlayerController.PlayerData.IsImpostor = true;
                #endregion

                CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("JesterWonRecv", $"{true}");
            } else if(e.Command == "JesterWonRecv" && CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.IsDecidingClient)
            {
                ReceivedBy++;

                if (ReceivedBy != PlayerController.AllPlayerControls.Count)
                    return;

                bool ShouldCallEnd = PlayerController.AllPlayerControls.Where(x => x.PlayerData.IsImpostor).Count() < PlayerController.AllPlayerControls.Where(x => !x.PlayerData.IsImpostor).Count();

                if (ShouldCallEnd)
                {
                    Patching.Patch_ShipStatusClass_RpcEndGame.CanEndGame = true;
                    ShipStatusClass.PLBGOMIEONF(GameEndReasonEnum.ImpostorByKill, false);
                }
            }
        }
    }
}
