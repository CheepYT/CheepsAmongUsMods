using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfusionGameMode
{
    public class ConfusionGameMode : GameMode
    {
        public ConfusionGameMode() : base(13, "Confusion") {
            RpcManager.RpcReceived += RpcManager_RpcReceived;
        }

        private void RpcManager_RpcReceived(object sender, RpcEventArgs e)
        {
            if (e.Command != Confusion.ConfusionRpc)
                return;

            var cmd = e.MessageReader.ReadByte();

            switch (cmd)
            {
                case 0:
                    {
                        var playerId = e.MessageReader.ReadByte();
                        var colorId = e.MessageReader.ReadByte();
                        var name = e.MessageReader.ReadString();

                        var player = PlayerController.FromPlayerId(playerId);

                        if (player.IsLocalPlayer)
                        {
                            player.RpcSetColor((ColorType)colorId);
                            player.RpcSetHat(HatType.NoHat);
                            player.RpcSetName(name);
                            player.RpcSetSkin(SkinType.None);
                            player.RpcSetPet(PetType.NoPet);
                        }

                        break;
                    }
            }
        }

        public override void OnStart()
        {
            base.OnStart();

            if (CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                Task.Run(async () =>
                {
                    await Task.Delay(1500); // Wait a bit

                    Random random = new Random();

                    List<ColorType> AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));
                    List<ColorType> AvailableNames = new List<ColorType>(AvailableColors);

                    foreach (var player in PlayerController.AllPlayerControls)
                    {
                        #region ---------- This is required, if more than 12 players are online, as the game only offers 12 colors ----------
                        if (AvailableColors.Count == 0 || AvailableNames.Count == 0)
                        {
                            AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));
                            AvailableNames = new List<ColorType>(AvailableColors);
                        }
                        #endregion

                        #region ---------- Select random color and name ----------
                        var color = AvailableColors[random.Next(AvailableColors.Count)];
                        var name = AvailableNames[random.Next(AvailableNames.Count)];

                        AvailableColors.Remove(color);
                        AvailableNames.Remove(name);
                        #endregion

                        #region ---------- As long as the name matches the color, change it ----------
                        while (color == name)
                            name = AvailableNames[random.Next(AvailableNames.Count)];
                        #endregion

                        if (player.IsLocalPlayer)
                        {
                            player.RpcSetColor(color);
                            player.RpcSetHat(HatType.NoHat);
                            player.RpcSetName(name.ToString());
                            player.RpcSetSkin(SkinType.None);
                            player.RpcSetPet(PetType.NoPet);
                        } else
                        {
                            var writer = RpcManager.StartRpc(Confusion.ConfusionRpc);
                            writer.Write((byte)0);
                            writer.Write(player.PlayerId);
                            writer.Write((byte)color);
                            writer.Write(name.ToString());
                            writer.EndMessage();
                        }

                    }
                });
        }
    }
}
