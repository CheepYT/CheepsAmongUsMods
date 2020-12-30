using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsApi.API.Events;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;
using GameEndReasonEnum = AIMMJPEOPEC;
using ShipStatusClass = HLBNNHFCNAJ;

namespace BattleRoyale
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class BattleRoyale : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusbattleroyale";
        public const string PluginName = "BattleRoyaleGameMode";
        public const string PluginVersion = "1.4.1";

        public const string GameModeName = "BattleRoyale";

        public const string Delimiter = "\n----------";

        public const float KillDistance = 1.2f;

        public static ManualLogSource _logger = null;

        private static bool RandomStartLocation = true;

        public static int LastKilled = 0;

        private static bool Started = false;

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

            #region ---------- Start Locations ----------
            Dictionary<MapType, Vector2[]> MapLocations = new Dictionary<MapType, Vector2[]>();

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

            int MaxPlayerCount = 10;

            #region ---------- Game Started Battle Royal ----------
            GameStartedEvent.Listener += () =>
            {
                if (!IsThisGameModeSelected)
                    return;

                Task.Run(async () =>
                {
                    LastKilled = 0;

                    Started = false;
                    Patching.PatchFinishEndGame.CanEndGame = false;

                    await Task.Delay(2500);

                    LastKilled = Functions.GetUnixTime();

                    Started = true;

                    //PlayerController.GetAllPlayers().Where(x => x.PlayerData.IsImpostor).ToList()[0].PlayerData.IsImpostor = false; // Unset existing impostor for this client

                    MaxPlayerCount = PlayerController.GetAllPlayers().Count;

                    PlayerController.GetLocalPlayer().ClearTasks();

                    PlayerHudManager.SetVictoryText($"{Functions.ColorCyan}Victory Royale");

                    #region ---------- Random Start Location ----------
                    if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer() && MapLocations.ContainsKey(GameOptions.Map) && RandomStartLocation)
                    {
                        List<Vector2> Locations = new List<Vector2>(MapLocations[GameOptions.Map]);

                        System.Random rnd = new System.Random();

                        foreach (var player in PlayerController.GetAllPlayers())
                        {
                            if (Locations.Count == 0)
                                Locations.AddRange(MapLocations[GameOptions.Map]);

                            var location = Locations[rnd.Next(Locations.Count)];
                            Locations.Remove(location);

                            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("StartLocation", $"{player.NetId};{location.x};{location.y}");
                        }
                    }
                    #endregion
                });
            };
            #endregion

            #region ---------- Game Running Battle Royal ----------
            HudUpdateEvent.Listener += () =>
            {
                try
                {
                    if (!IsThisGameModeActive)
                        return;

                    #region ----- Display new task text -----
                    string ToDisplay = "----- [ff7b00ff]Battle Royale[] -----\n" +
                                        "Be the last man standing!\n" +
                                        "\n" +
                                        $"Players left: { PlayerController.GetAllPlayers().Where(x => !x.PlayerData.IsDead).Count() }/{ MaxPlayerCount }";

                    PlayerHudManager.TaskText = ToDisplay + Delimiter; // Current text
                    #endregion

                    #region ----- Display Kill Button -----
                    PlayerHudManager.HudManager.ReportButton.enabled = false;    // Disable report button
                    PlayerHudManager.HudManager.ReportButton.gameObject.SetActive(false);    // Disable report button
                    PlayerHudManager.HudManager.ReportButton.renderer.color = new Color(1, 1, 1, 0);    // Hide report button

                    PlayerHudManager.HudManager.KillButton.gameObject.SetActive(!PlayerController.GetLocalPlayer().PlayerData.IsDead); // Activate Kill Button
                    PlayerHudManager.HudManager.KillButton.isActive = !PlayerController.GetLocalPlayer().PlayerData.IsDead; // Activate Kill Button

                    #region --- Update Cooldown ---
                    if (GameOptions.KillCooldown - (Functions.GetUnixTime() - LastKilled) <= 0)
                        LastKilled = 0;

                    if (LastKilled != 0)
                        PlayerHudManager.HudManager.KillButton.SetCoolDown(GameOptions.KillCooldown - (Functions.GetUnixTime() - LastKilled), GameOptions.KillCooldown);
                    else
                        PlayerHudManager.HudManager.KillButton.SetCoolDown(0, GameOptions.KillCooldown);
                    #endregion

                    #region ----- End Game If Required -----
                    if (PlayerController.GetAllPlayers().Where(x => !x.PlayerData.IsDead).Count() == 1 && CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer() && Started)
                    {
                        Started = false;

                        var winner = PlayerController.GetAllPlayers().Where(x => !x.PlayerData.IsDead).ToList()[0];

                        CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("SetWinner", $"{winner.NetId}");
                    }
                    #endregion

                    if (!Started)
                        return;

                    #region ----- Get Closest Player -----
                    IEnumerable<PlayerController> AvailablePlayers = PlayerController.GetAllPlayers().Where(x => !x.PlayerData.IsDead && !x.AmPlayerController());

                    PlayerController closest = AvailablePlayers.ToList()[0];

                    foreach (var player in AvailablePlayers)
                    {
                        float DistOld = Vector2.Distance(closest.Position, PlayerController.GetLocalPlayer().Position);
                        float DistNew = Vector2.Distance(player.Position, PlayerController.GetLocalPlayer().Position);

                        if (DistNew < DistOld)
                            closest = player;
                    }
                    #endregion

                    #region --- Update Target ---
                    if (Vector2.Distance(closest.Position, PlayerController.GetLocalPlayer().Position) <= KillDistance)
                    {
                        PlayerHudManager.HudManager.KillButton.SetTarget(closest.PlayerControl);
                        PlayerHudManager.HudManager.KillButton.CurrentTarget = closest.PlayerControl;
                        Patching.PatchPerformKill.Target = closest;
                    } else
                    {
                        Patching.PatchPerformKill.Target = null;
                    }
                    #endregion
                    #endregion
                }
                catch { }
            };
            #endregion
        }

        private void Patching_ServerCommandExecuted(object sender, CheepsAmongUsBaseMod.ServerCommandEventArgs e)
        {
            if (!IsThisGameModeSelected)
                return;

            if(e.Command == "SetWinner")
            {
                var winner = PlayerController.FromNetId(uint.Parse(e.Value));

                winner.PlayerData.IsDead = false;

                foreach (var player in PlayerController.GetAllPlayers().Where(x => !x.Equals(winner)))
                    player.PlayerData.IsDead = true;

                PlayerHudManager.SetDefeatText($"{Functions.ColorOrange}" +
                    $"{winner.PlayerData.PlayerName}");

                winner.PlayerData.IsImpostor = true;

                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(2500); // Wait a bit, for others to have received message
                        Patching.PatchFinishEndGame.CanEndGame = true;
                        ShipStatusClass.PLBGOMIEONF(GameEndReasonEnum.ImpostorByKill, false); // RpcEndGame	// RVA: 0x9819F0 Offset: 0x9801F0 VA: 0x109819F0
                    });
                }
            }

            if (e.Command == "StartLocation")
            {
                var playerId = uint.Parse(e.Value.Split(';')[0]);
                var x = float.Parse(e.Value.Split(';')[1]);
                var y = float.Parse(e.Value.Split(';')[2]);

                var player = PlayerController.FromNetId(playerId);

                if (player.AmPlayerController())
                    player.RpcSnapTo(new Vector2(x, y));
            }

            if (e.Command == "KillPlayer")
            {
                var target = PlayerController.FromNetId(uint.Parse(e.Value.Split(';')[0]));
                var killer = PlayerController.FromNetId(uint.Parse(e.Value.Split(';')[1]));

                if (!killer.AmPlayerController())
                    killer.PlayerControl.MurderPlayer(target.PlayerControl);

                target.PlayerData.IsDead = true;
            }
        }
    }
}
