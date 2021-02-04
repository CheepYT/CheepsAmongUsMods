using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.CustomGameOptions;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using DeconDoor = NBEJDLGKDGA;
using ShipStatusClass = HLBNNHFCNAJ;
using GameEndReason = AIMMJPEOPEC;

namespace BattleRoyalGameMode
{
    public class BattleRoyaleGameMode : GameMode
    {
        public const float KillDistance = 1.2f;

        public bool RandomStartLocation = true;
        public int LastKilled = 0;
        public int MaxPlayerCount = 10;
        private bool Started = false;

        private readonly Dictionary<MapType, Vector2[]> MapLocations = new Dictionary<MapType, Vector2[]>();

        public BattleRoyaleGameMode() : base(11, BattleRoyale.GameModeName) {

            #region ---------- Add Custom Options ----------
            new CustomNumberOption($"{Functions.ColorPurple}{GameModeName}", new string[] { "Options" });

            new CustomToggleOption("Random Start Location")
            {
                Value = true
            }.ValueChanged += BattleRoyaleGameMode_ValueChanged;
            #endregion

            #region ---------- Rpc Received ----------
            RpcManager.RpcReceived += RpcManager_RpcReceived;
            #endregion

            #region ---------- Start Locations ----------
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
        }

        #region ------------------------------ Events ------------------------------
        private void RpcManager_RpcReceived(object sender, RpcEventArgs e)
        {
            if (e.Command != BattleRoyale.BattleRoyalRpc)
                return;

            var command = e.MessageReader.ReadByte();

            switch (command)
            {
                case (byte)BattleRoyale.CustomRpcCalls.UpdateRandomStartLocation:
                    {
                        RandomStartLocation = Convert.ToBoolean(e.MessageReader.ReadByte());
                        break;
                    }

                case (byte)BattleRoyale.CustomRpcCalls.SendStartLocation:
                    {
                        var playerId = e.MessageReader.ReadByte();
                        float x = e.MessageReader.ReadSingle();
                        float y = e.MessageReader.ReadSingle();

                        var loc = new Vector2(x, y);

                        var player = PlayerController.FromPlayerId(playerId);

                        if (player.IsLocalPlayer)
                            player.RpcSnapTo(loc);
                        break;
                    }

                case (byte)BattleRoyale.CustomRpcCalls.SetWinner:
                    {
                        var winner = PlayerController.FromPlayerId(e.MessageReader.ReadByte());

                        RolePlayer MyRole = new RolePlayer(PlayerController.LocalPlayer, "Default");

                        MyRole.RoleOutro.UseRoleOutro = true;

                        if (winner.IsLocalPlayer)
                        {
                            MyRole.RoleOutro.WinText = "Victory Royale";
                            MyRole.RoleOutro.WinTextColor = new Color(255/255f, 174/255f, 0/255f);
                            MyRole.RoleOutro.BackgroundColor = new Color(1f, 0.8f, 0f);
                        } else
                        {
                            MyRole.RoleOutro.WinText = winner.PlayerData.PlayerName;
                            MyRole.RoleOutro.WinTextColor = new Color(255 / 255f, 174 / 255f, 0 / 255f);
                            MyRole.RoleOutro.BackgroundColor = new Color(1f, 0f, 0f);
                        }

                        winner.PlayerData.IsImpostor = true;
                        break;
                    }

                case (byte)BattleRoyale.CustomRpcCalls.KillPlayer:
                    {
                        var killer = PlayerController.FromPlayerId(e.MessageReader.ReadByte());
                        var target = PlayerController.FromPlayerId(e.MessageReader.ReadByte());

                        if (!killer.PlayerData.IsDead && !target.PlayerData.IsDead && CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                        {
                            RpcManager.SendRpc(BattleRoyale.BattleRoyalRpc, new byte[] { (byte)BattleRoyale.CustomRpcCalls.MurderPlayer, killer.PlayerId, target.PlayerId });

                            killer.PlayerControl.MurderPlayer(target.PlayerControl);
                            target.PlayerData.IsDead = true;
                        }
                        break;
                    }

                case (byte)BattleRoyale.CustomRpcCalls.MurderPlayer:
                    {
                        var killer = PlayerController.FromPlayerId(e.MessageReader.ReadByte());
                        var target = PlayerController.FromPlayerId(e.MessageReader.ReadByte());

                        killer.PlayerControl.MurderPlayer(target.PlayerControl);
                        target.PlayerData.IsDead = true;
                        break;
                    }
            }
        }

        private void BattleRoyaleGameMode_ValueChanged(object sender, CustomToggleOption.CustomToggleOptionEventArgs e)
        {
            RandomStartLocation = e.ToggleOption.Value;

            RpcManager.SendRpc(BattleRoyale.BattleRoyalRpc, new byte[] { (byte)BattleRoyale.CustomRpcCalls.UpdateRandomStartLocation, Convert.ToByte(RandomStartLocation) });
        }
        #endregion

        public override void OnStart()
        {
            base.OnStart();

            Task.Run(async () =>
            {
                await Task.Delay(2500);

                MaxPlayerCount = PlayerController.AllPlayerControls.Count;

                Started = true;
                LastKilled = 0;

                PlayerController.LocalPlayer.ClearTasks();

                #region ---------- Send Start Location ----------
                if (RandomStartLocation && CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient && MapLocations.ContainsKey(GameOptions.Map))
                {
                    List<Vector2> Locations = new List<Vector2>(MapLocations[GameOptions.Map]);
                    System.Random rnd = new System.Random();

                    foreach (var player in PlayerController.AllPlayerControls)
                    {
                        if (Locations.Count == 0)
                            Locations.AddRange(MapLocations[GameOptions.Map]); // If there are not enough start locations, refill list

                        var location = Locations[rnd.Next(Locations.Count)];
                        Locations.Remove(location);

                        if (!player.IsLocalPlayer)
                        {
                            var writer = RpcManager.StartRpc(BattleRoyale.BattleRoyalRpc);
                            writer.Write((byte)BattleRoyale.CustomRpcCalls.SendStartLocation);
                            writer.Write(player.PlayerId);
                            writer.Write(location.x);
                            writer.Write(location.y);
                            writer.EndMessage();
                        } else
                        {
                            player.RpcSnapTo(location);
                        }
                    }
                }
                #endregion

                #region ---------- Open All Doors ----------
                await Task.Delay(2500);

                foreach (var door in UnityEngine.Object.FindObjectsOfType<DeconDoor>())
                    door.SetDoorway(true);
                #endregion

                LastKilled = Functions.GetUnixTime();
            });
        }

        public override void Loop()
        {
            base.Loop();

            GameModeSettingsAddition = $"\nRandom Start Location: {Functions.ColorPurple}{(RandomStartLocation ? "On" : "Off")}";

            if (!IsInGame)
                return;

            #region ----- Display new task text -----
            string ToDisplay = "----- [ff7b00ff]Battle Royale[] -----\n" +
                                "Be the last man standing!\n" +
                                "\n" +
                                $"Players left: { PlayerController.AllPlayerControls.Where(x => !x.PlayerData.IsDead).Count() }/{ MaxPlayerCount }";

            PlayerHudManager.TaskText = ToDisplay + "\n--------------------"; // Current text
            #endregion

            #region ----- Display Kill Button -----
            PlayerHudManager.HudManager.KillButton.gameObject.SetActive(!PlayerController.LocalPlayer.PlayerData.IsDead); // Activate Kill Button
            PlayerHudManager.HudManager.KillButton.isActive = !PlayerController.LocalPlayer.PlayerData.IsDead; // Activate Kill Button

            PlayerHudManager.HudManager.KillButton.transform.position = PlayerHudManager.HudManager.UseButton.transform.position;   // Move the Kill Button

            PlayerHudManager.HudManager.ReportButton.enabled = false;    // Disable report button
            PlayerHudManager.HudManager.ReportButton.gameObject.SetActive(false);    // Disable report button
            PlayerHudManager.HudManager.ReportButton.renderer.color = new Color(1, 1, 1, 0);    // Hide report button

            PlayerHudManager.HudManager.UseButton.enabled = false;    // Disable use button
            PlayerHudManager.HudManager.UseButton.gameObject.SetActive(false);    // Disable use button
            PlayerHudManager.HudManager.UseButton.UseButton.color = new Color(1, 1, 1, 0);    // Hide use button

            #region --- Update Cooldown ---
            if (GameOptions.KillCooldown - (Functions.GetUnixTime() - LastKilled) <= 0)
                LastKilled = 0;

            if (LastKilled != 0)
                PlayerHudManager.HudManager.KillButton.SetCoolDown(GameOptions.KillCooldown - (Functions.GetUnixTime() - LastKilled), GameOptions.KillCooldown);
            else
                PlayerHudManager.HudManager.KillButton.SetCoolDown(0, GameOptions.KillCooldown);
            #endregion

            #region ----- End Game If Required -----
            if (PlayerController.AllPlayerControls.Where(x => !x.PlayerData.IsDead).Count() == 1 && Started)
            {
                Started = false;

                if (!CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                    return;

                Task.Run(async () =>
                {
                    await Task.Delay(2500);
                    Patching.PatchFinishEndGame.CanEndGame = true;
                    ShipStatusClass.PLBGOMIEONF(GameEndReason.ImpostorByKill, false);
                });

                var winner = PlayerController.AllPlayerControls.Where(x => !x.PlayerData.IsDead).First();

                winner.PlayerData.IsImpostor = true;

                RolePlayer MyRole = new RolePlayer(PlayerController.LocalPlayer, "Default");

                MyRole.RoleOutro.UseRoleOutro = true;

                MyRole.RoleOutro.UseRoleOutro = true;

                if (winner.IsLocalPlayer)
                {
                    MyRole.RoleOutro.WinText = "Victory Royale";
                    MyRole.RoleOutro.WinTextColor = new Color(255 / 255f, 174 / 255f, 0 / 255f);
                    MyRole.RoleOutro.BackgroundColor = new Color(1f, 0.8f, 0f);
                }
                else
                {
                    MyRole.RoleOutro.WinText = winner.PlayerData.PlayerName;
                    MyRole.RoleOutro.WinTextColor = new Color(255 / 255f, 174 / 255f, 0 / 255f);
                    MyRole.RoleOutro.BackgroundColor = new Color(1f, 0f, 0f);
                }

                RpcManager.SendRpc(BattleRoyale.BattleRoyalRpc, new byte[] { (byte)BattleRoyale.CustomRpcCalls.SetWinner, winner.PlayerId });
            }
            #endregion

            if (!Started)
                return;

            #region ----- Get Closest Player -----
            IEnumerable<PlayerController> AvailablePlayers = PlayerController.AllPlayerControls.Where(x => !x.PlayerData.IsDead && !x.IsLocalPlayer);

            PlayerController closest = AvailablePlayers.ToList()[0];

            foreach (var player in AvailablePlayers)
            {
                float DistOld = Vector2.Distance(closest.Position, PlayerController.LocalPlayer.Position);
                float DistNew = Vector2.Distance(player.Position, PlayerController.LocalPlayer.Position);

                if (DistNew < DistOld)
                    closest = player;
            }
            #endregion

            #region --- Update Target ---
            if (Vector2.Distance(closest.Position, PlayerController.LocalPlayer.Position) <= KillDistance)
            {
                PlayerHudManager.HudManager.KillButton.SetTarget(closest.PlayerControl);
                PlayerHudManager.HudManager.KillButton.CurrentTarget = closest.PlayerControl;
                Patching.PatchPerformKill.Target = closest;
            }
            else
            {
                Patching.PatchPerformKill.Target = null;
            }
            #endregion
            #endregion
        }

        public override void SyncSettings()
        {
            RpcManager.SendRpc(BattleRoyale.BattleRoyalRpc, new byte[] { (byte)BattleRoyale.CustomRpcCalls.UpdateRandomStartLocation, Convert.ToByte(RandomStartLocation) });
            
            base.SyncSettings();
        }

        public override void ResetValues()
        {
            Started = false;
            LastKilled = 0;
            Patching.PatchFinishEndGame.CanEndGame = false;

            base.ResetValues();
        }
    }
}
