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

namespace TeleportationGameMode
{
    public class TeleportationGameMode : GameMode
    {
        Dictionary<MapType, Vector2[]> MapLocations = new Dictionary<MapType, Vector2[]>();

        private static System.Random RandomGen = new System.Random();

        public static int TeleportationDelay = 60;
        public const string Delimiter = "\n----------\n";

        private static int LastTeleported = 0;

        public TeleportationGameMode() : base(16, "Teleportation")
        {
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

            #region ---------- Add Custom Options ----------
            new CustomNumberOption($"{Functions.ColorPurple}{GameModeName}", new string[] { "Options" });

            var delay = new CustomNumberOption("Teleportation Delay", CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings);
            delay.ValueChanged += Delay_ValueChanged;
            delay.Value = CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings.ToList().IndexOf(TeleportationDelay.ToString());
            #endregion

            RpcManager.RpcReceived += RpcManager_RpcReceived;
        }

        private void RpcManager_RpcReceived(object sender, RpcEventArgs e)
        {
            if (e.Command != Teleportation.TeleportationRpc)
                return;

            var cmd = e.MessageReader.ReadByte();

            switch (cmd) {
                case 0:
                    {
                        TeleportationDelay = e.MessageReader.ReadInt32();
                        break;
                    }

                case 1:
                    {
                        Teleport();
                        break;
                    }
            }
        }

        private void Teleport()
        {
            LastTeleported = Functions.GetUnixTime();

            if (PlayerController.LocalPlayer.PlayerControl.inVent || PlayerController.LocalPlayer.PlayerData.IsDead) // Dont teleport if player is dead or in a vent
                return;

            Vector2[] positions = MapLocations[GameOptions.Map];

            Vector2 toTp = positions[RandomGen.Next(0, positions.Length)];

            var ctrl = PlayerController.LocalPlayer;

            ctrl.RpcSnapTo(toTp);
        }

        private void SyncDelay()
        {
            var writer = RpcManager.StartRpc(Teleportation.TeleportationRpc);
            writer.Write((byte)0);
            writer.Write(TeleportationDelay);
            writer.EndMessage();
        }

        private void Delay_ValueChanged(object sender, CustomNumberOption.CustomNumberOptionEventArgs e)
        {
            int value = int.Parse(e.NumberOption.Selected);

            TeleportationDelay = value;
            SyncDelay();
        }

        public override void OnStart()
        {
            base.OnStart();

            SyncDelay();
            LastTeleported = Functions.GetUnixTime();
        }

        public override void SyncSettings()
        {
            base.SyncSettings();

            SyncDelay();
        }

        public override void Loop()
        {
            base.Loop();

            GameModeSettingsAddition = $"\nTeleportation Delay: {Functions.ColorPurple}{TeleportationDelay}s[]";

            if (!IsInGame)
                return;

            string toDisplay = "----- [7a31f7ff]Teleportation[] -----\n";

            toDisplay += $"Teleportation in [11c5edff]{ TeleportationDelay - (Functions.GetUnixTime() - LastTeleported) }s[]";

            if (TeleportationDelay - (Functions.GetUnixTime() - LastTeleported) <= 0 && LastTeleported != 0)
            {
                LastTeleported = Functions.GetUnixTime();

                if (MapLocations.ContainsKey(GameOptions.Map) && CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                {
                    Teleport();

                    RpcManager.SendRpc(Teleportation.TeleportationRpc, 1);
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
        }
    }
}
