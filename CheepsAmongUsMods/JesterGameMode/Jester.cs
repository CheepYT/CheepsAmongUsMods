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
using System.Threading.Tasks;
using UnityEngine;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;
using ShipStatusClass = HLBNNHFCNAJ;
using GameEndReason = AIMMJPEOPEC;

namespace JesterGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class Jester : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusjester";
        public const string PluginName = "JesterGameMode";
        public const string PluginVersion = "1.4.1";

        public const string GameModeName = "Jester";

        public const string Delimiter = "\n----------";

        public static ManualLogSource _logger = null;

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

        internal static PlayerController TheJester = null;
        internal static bool HasJesterWon = false;
        internal static bool TasksCompleted = false;
        private static bool GameEnded = false;
        internal static bool Started = false;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.InstalledGameModes.Add(GameModeName);

            CheepsAmongUsBaseMod.Patching.ServerCommandExecuted += Patching_ServerCommandExecuted;

            #region ---------- Enable Harmony Patching ----------
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            System.Random rnd = new System.Random();

            #region ---------- GameMode Started ----------
            GameStartedEvent.Listener += () =>
            {
                if (!IsThisGameModeSelected)
                    return;

                HasJesterWon = false; // Reset bool
                GameEnded = false;
                TasksCompleted = false;
                Started = false;
                TheJester = null;

                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                {
                    Task.Run(async () =>
                    {
                        while (PlayerController.GetAllPlayers().Count == 1) 
                            await Task.Delay(1);

                        List<PlayerController> AvailablePlayers = PlayerController.GetAllPlayers().Where(x => !x.PlayerData.IsImpostor).ToList();

                        TheJester = AvailablePlayers[rnd.Next(AvailablePlayers.Count)];

                        CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("SetJester", $"{TheJester.NetId}");
                        Started = true;
                    });
                }
            };
            #endregion

            #region ---------- GameMode Running ----------
            HudUpdateEvent.Listener += () =>
            {
                try
                {
                    if (!IsThisGameModeActive || TheJester == null)
                        return;

                    if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer() && Started)
                    {
                        int numImpostors = PlayerController.GetAllPlayers().Where(x => x.PlayerData.IsImpostor && !x.PlayerData.IsDead).Count();
                        int numAlive = PlayerController.GetAllPlayers().Where(x => !x.PlayerData.IsImpostor && !x.PlayerData.IsDead).Count();

                        if (!HasJesterWon && !GameEnded)
                        {
                            if (numImpostors >= numAlive)
                            {
                                GameEnded = true;
                                Patching.PatchFinishEndGame.CanEndGame = true;
                                ShipStatusClass.PLBGOMIEONF(GameEndReason.ImpostorByKill, false);
                            } else if(numImpostors == 0 || TasksCompleted)
                            {
                                Patching.PatchFinishEndGame.CanEndGame = true;
                                CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("JesterLoose", $"{GameEndReason.HumansByVote}");
                            }
                        }
                    }

                    if (!TheJester.AmPlayerController())
                        return;

                    string toDisplay = "----- [7a31f7ff]Jester[] -----\n";
                    toDisplay += "You are [410dffff]The Jester[]\n" +
                                 "Trick the crewmates into thinking\n" +
                                 "that you are an [ff0000ff]Impostor[]\n" +
                                 "Your goal is to [ff0000ff]Get Exiled[]";

                    PlayerHudManager.TaskText = toDisplay + Delimiter;
                } catch { }
            };
            #endregion
        }

        private void Patching_ServerCommandExecuted(object sender, CheepsAmongUsBaseMod.ServerCommandEventArgs e)
        {
            if (!IsThisGameModeSelected)
                return;

            if(e.Command == "SetJester")
            {
                TheJester = PlayerController.FromNetId(uint.Parse(e.Value));
                TheJester.ClearTasks();

                if (TheJester.AmPlayerController())
                {
                    PlayerHudManager.SetVictoryText($"{Functions.ColorPurple}Victory");
                    TheJester.PlayerControl.nameText.Color = new Color(0.74901960784f, 0, 1f);
                }
            } else if(e.Command == "JesterWin")
            {
                HasJesterWon = true;
                TheJester.PlayerData.IsDead = false;
                TheJester.PlayerData.IsImpostor = true;

                PlayerHudManager.SetDefeatText($"{Functions.ColorPurple}Defeat");

                foreach (var player in PlayerController.GetAllPlayers().Where(x => !x.Equals(TheJester)))
                {
                    player.PlayerData.IsImpostor = false;
                    player.PlayerData.IsDead = true;
                }

                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(1500);
                        Patching.PatchFinishEndGame.CanEndGame = true;
                        ShipStatusClass.PLBGOMIEONF(GameEndReason.ImpostorByVote, false);
                    });
                }
            } else if(e.Command == "JesterLoose")
            {
                GameEndReason Reason = (GameEndReason)Enum.Parse(typeof(GameEndReason), e.Value);

                if (Reason == GameEndReason.HumansByVote)
                    TheJester.PlayerData.IsImpostor = true;

                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(1500);
                        Patching.PatchFinishEndGame.CanEndGame = true;
                        ShipStatusClass.PLBGOMIEONF(Reason, false);
                    });
                }
            }
        }
    }
}
