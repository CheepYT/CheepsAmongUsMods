using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsApi.API.Events;
using CheepsAmongUsBaseMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;

namespace ConfusionGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class Confusion : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusconfusion";
        public const string PluginName = "ConfusionGameMode";
        public const string PluginVersion = "1.0.5";

        public const string GameModeName = "Confusion";

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

            Patching.ServerCommandExecuted += Patching_ServerCommandExecuted;

            GameStartedEvent.Listener += () =>
            {
                if (!IsThisGameModeActive)
                    return;

                #region --------- Update player styles ---------
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.IsDecidingClient)
                    Task.Run(async () =>
                    {
                        await Task.Delay(2500); // Wait a bit

                        Random random = new Random();

                        List<ColorType> AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));
                        List<ColorType> AvailableNames = new List<ColorType>(AvailableColors);

                        foreach(var player in PlayerController.AllPlayerControls)
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

                            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("SetPlayerStyle", $"{player.PlayerData.PlayerName};{color};{name}");
                        }
                    });
                #endregion
            };
        }

        private void Patching_ServerCommandExecuted(object sender, ServerCommandEventArgs e)
        {
            if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode != GameModeName)
                return;

            #region ---------- Set the player style ----------
            if (e.Command == "SetPlayerStyle")
            {
                string playerName = e.Value.Split(';')[0];
                string color = e.Value.Split(';')[1];
                string name = e.Value.Split(';')[2];

                var player = PlayerController.FromName(playerName);

                if (player.IsLocalPlayer)
                {
                    // Only the player can set its own style
                    ColorType colorType = (ColorType)Enum.Parse(typeof(ColorType), color);

                    player.RpcSetColor(colorType);
                    player.RpcSetHat(HatType.NoHat);
                    player.RpcSetSkin(SkinType.None);
                    player.RpcSetPet(PetType.NoPet);
                    player.RpcSetName(name);
                }
            }
            #endregion
        }
    }
}
