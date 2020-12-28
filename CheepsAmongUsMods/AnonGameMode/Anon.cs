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
using System.Text;
using System.Threading.Tasks;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;

using GameStartManager = ANKMIOIMNFE;
using System.Reflection;

namespace AnonGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class Anon : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusanon";
        public const string PluginName = "AnonGameMode";
        public const string PluginVersion = "1.0.0";

        public const string GameModeName = "Anon";

        public const string Delimiter = "\n----------\n";

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

            GameStartedEvent.Listener += () =>
            {
                if (!IsThisGameModeActive)
                    return;

                #region --------- Unset all style options ---------
                Task.Run(async () =>
                {
                    await Task.Delay(2500); // Wait a bit

                    var player = PlayerController.GetLocalPlayer();

                    player.RpcSetColor(ColorType.Black);
                    player.RpcSetHat(HatType.NoHat);
                    player.RpcSetSkin(SkinType.None);
                    player.RpcSetPet(PetType.NoPet);
                    player.RpcSetName("");
                });
                #endregion
            };

            HudUpdateEvent.Listener += () =>
            {
                if (!IsThisGameModeActive)
                    return;

                string toDisplay = "[7a31f7ff]Anonymous[]\n";

                toDisplay += $"Everyone is anonymous.";

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
    }
}
