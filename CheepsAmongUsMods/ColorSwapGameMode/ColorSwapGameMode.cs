using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.CustomGameOptions;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSwapGameMode
{
    public class ColorSwapGameMode : GameMode
    {
        public static int SwapDelay = 60;
        public const string Delimiter = "\n----------\n";

        private static int LastSwapped = 0;

        public ColorSwapGameMode() : base(12, "Color Swap") {

            #region ---------- Add Custom Options ----------
            new CustomNumberOption($"{Functions.ColorPurple}{GameModeName}", new string[] { "Options" });

            var delay = new CustomNumberOption("Color Swap Delay", CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings);
            delay.ValueChanged += Delay_ValueChanged;
            delay.Value = CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings.ToList().IndexOf(SwapDelay.ToString());
            #endregion

            RpcManager.RpcReceived += RpcManager_RpcReceived;
        }

        #region -------------------- Events --------------------
        private void RpcManager_RpcReceived(object sender, RpcEventArgs e)
        {
            if(e.Command == ColorSwap.ColorSwapRpc)
            {
                var cmd = e.MessageReader.ReadByte();
                switch (cmd)
                {
                    case 0:
                        {
                            SwapDelay = e.MessageReader.ReadInt32();
                            break;
                        };

                    case 1:
                        {
                            var pid = e.MessageReader.ReadByte();
                            var color = e.MessageReader.ReadByte();

                            var player = PlayerController.FromPlayerId(pid);

                            if (player.IsLocalPlayer)
                            {
                                player.RpcSetColor((ColorType)color);
                                player.RpcSetHat(HatType.NoHat);
                                player.RpcSetName("");
                                player.RpcSetSkin(SkinType.None);
                                player.RpcSetPet(PetType.NoPet);
                            }

                            break;
                        }
                }
            }
        }

        private void SendSwapDelay()
        {
            var writer = RpcManager.StartRpc(ColorSwap.ColorSwapRpc);
            writer.Write((byte)0);
            writer.Write(SwapDelay);
            writer.EndMessage();
        }

        private void Delay_ValueChanged(object sender, CustomNumberOption.CustomNumberOptionEventArgs e)
        {
            SwapDelay = int.Parse(e.NumberOption.Selected);

            SendSwapDelay();
        }
        #endregion

        public override void SyncSettings()
        {
            base.SyncSettings();

            SendSwapDelay();
        }

        public override void OnStart()
        {
            base.OnStart();

            if (CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
            {
                SendSwapDelay();
            }

            LastSwapped = Functions.GetUnixTime();
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                LastSwapped = (int)(Functions.GetUnixTime() - SwapDelay);
            });
        }

        public override void Loop()
        {
            base.Loop();

            GameModeSettingsAddition = $"\nColor Swap Delay: {Functions.ColorPurple}{SwapDelay}s";

            if (!IsInGame)
                return;

            string toDisplay = $"----- {Functions.ColorYellow}Color Swap []-----\n";

            if (SwapDelay - (Functions.GetUnixTime() - LastSwapped) <= 0 && LastSwapped != 0)
            {
                LastSwapped = Functions.GetUnixTime();

                Random random = new Random();

                if (CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                {
                    List<ColorType> AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));

                    foreach (var player in PlayerController.AllPlayerControls)
                    {
                        #region ---------- This is required, if more than 12 players are online, as the game only offers 12 colors ----------
                        if (AvailableColors.Count == 0)
                            AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));
                        #endregion

                        #region ---------- Select random color ----------
                        var toSwitchTo = AvailableColors[random.Next(AvailableColors.Count)];
                        AvailableColors.Remove(toSwitchTo);
                        #endregion

                        if (player.IsLocalPlayer)
                        {
                            player.RpcSetColor(toSwitchTo);
                            player.RpcSetHat(HatType.NoHat);
                            player.RpcSetName("");
                            player.RpcSetSkin(SkinType.None);
                            player.RpcSetPet(PetType.NoPet);
                        } else
                        {
                            RpcManager.SendRpc(ColorSwap.ColorSwapRpc, new byte[] { 1, player.PlayerId, (byte)toSwitchTo });
                        }
                    }
                }
            }

            toDisplay += $"Color Swap in [11c5edff]{ SwapDelay - (Functions.GetUnixTime() - LastSwapped) }s[]";

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
