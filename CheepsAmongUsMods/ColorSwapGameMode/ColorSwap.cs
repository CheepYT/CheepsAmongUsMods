using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsApi.API.Events;
using CheepsAmongUsBaseMod;
using CheepsAmongUsBaseMod.ClientCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;

namespace ColorSwapGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class ColorSwap : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amonguscolorswap";
        public const string PluginName = "ColorSwapGameMode";
        public const string PluginVersion = "1.0.0";

        public static int SwapDelay = 60;
        public const string Delimiter = "\n----------\n";

        private static int LastSwapped = 0;

        public const string GameModeName = "ColorSwap";

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

            #region ---------- Manage Commands ----------
            HelpCommand.AvailableCommands.Add("/colorswapdelay <Int>", $"Set the color swap delay ({GameModeName})");
            
            ClientCommandManager.CommandExecuted += ClientCommandManager_CommandExecuted;
            Patching.ServerCommandExecuted += Patching_ServerCommandExecuted;
            #endregion

            GameStartedEvent.Listener += () =>
            {
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode != GameModeName || !CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                    return;

                Task.Run(async () =>
                {
                    await Task.Delay(2500);
                    CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("UpdateSwapDelay", $"{SwapDelay}");


                    Random random = new Random();
                    List<ColorType> AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));

                    foreach (var player in PlayerController.GetAllPlayers())
                    {
                        #region ---------- This is required, if more than 12 players are online, as the game only offers 12 colors ----------
                        if (AvailableColors.Count == 0)
                            AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));
                        #endregion

                        #region ---------- Select random color ----------
                        var toSwitchTo = AvailableColors[random.Next(AvailableColors.Count)];
                        #endregion

                        CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("SwapNow", $"{player.NetId};{toSwitchTo}");
                    }

                });
            };

            #region ---------- Color Swap Mode ----------
            HudUpdateEvent.Listener += () =>
            {
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode == GameModeName)
                    CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.GameSettingsAddition = $"\nColor Swap Delay: {Functions.ColorPurple}{SwapDelay}s[]";

                if (!IsThisGameModeActive || LastSwapped == 0)
                {
                    LastSwapped = Functions.GetUnixTime();
                    return;
                }

                string toDisplay = $"----- {Functions.ColorYellow}Color Swap []-----\n";

                if (SwapDelay - (Functions.GetUnixTime() - LastSwapped) <= 0 && LastSwapped != 0)
                {
                    LastSwapped = Functions.GetUnixTime();

                    Random random = new Random();

                    if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                    {
                        List<ColorType> AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));

                        foreach (var player in PlayerController.GetAllPlayers())
                        {
                            #region ---------- This is required, if more than 12 players are online, as the game only offers 12 colors ----------
                            if (AvailableColors.Count == 0)
                                AvailableColors = new List<ColorType>((IEnumerable<ColorType>)Enum.GetValues(typeof(ColorType)));
                            #endregion

                            #region ---------- Select random color ----------
                            var toSwitchTo = AvailableColors[random.Next(AvailableColors.Count)];
                            AvailableColors.Remove(toSwitchTo);
                            #endregion

                            CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("SwapNow", $"{player.NetId};{toSwitchTo}");
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
            };
            #endregion
        }

        private void ClientCommandManager_CommandExecuted(object sender, CommandEventArgs e)
        {
            if (e.Command.ToLower().Equals("/colorswapdelay"))
            {
                e.Handled = true;

                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.AmDecidingPlayer())
                    try
                    {
                        int delay = int.Parse(e.Arguments[1]);

                        SwapDelay = delay;

                        CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("UpdateSwapDelay", $"{SwapDelay}");

                        PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                            $"{Functions.ColorLime}The color swap interval has been updated to {Functions.ColorPurple}{SwapDelay}s"
                            ); //Send syntax to player
                    }
                    catch
                    {
                        PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                            $"{Functions.ColorRed}Syntax[]: {Functions.ColorPurple}/colorswapdelay <Int>"
                            ); //Send syntax to player
                    }
                else
                    PlayerHudManager.AddChat(PlayerController.GetLocalPlayer(),
                        $"{Functions.ColorRed}Sorry, but only {Functions.ColorCyan}{CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.GetDecidingPlayer().PlayerData.PlayerName} " +
                        $"{Functions.ColorRed}can change the color swap interval."
                        ); //Send syntax to player
            }
        }

        private void Patching_ServerCommandExecuted(object sender, ServerCommandEventArgs e)
        {
            if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode != GameModeName)
                return;

            if (e.Command == "UpdateSwapDelay")
            {
                SwapDelay = int.Parse(e.Value);
            }

            if(e.Command == "SwapNow")
            {
                LastSwapped = Functions.GetUnixTime();

                var netId = e.Value.Split(';')[0];
                var color = (ColorType)Enum.Parse(typeof(ColorType), e.Value.Split(';')[1]);

                var player = PlayerController.FromNetId(uint.Parse(netId));

                if (player.AmPlayerController())
                {
                    player.RpcSetHat(HatType.NoHat);
                    player.RpcSetSkin(SkinType.None);
                    player.RpcSetPet(PetType.NoPet);
                    player.RpcSetName("");
                    player.RpcSetColor(color);
                }
            }
        }
    }
}
