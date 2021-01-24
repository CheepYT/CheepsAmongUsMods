﻿using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsApi.API.Events;
using System.Threading.Tasks;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;

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
        public const string PluginVersion = "1.0.53";

        public const string GameModeName = "Anon";

        public const string Delimiter = "\n----------\n";

        public static ManualLogSource _logger = null;

        public static bool IsThisGameModeActive
        {
            get
            {
                if (CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.CurrentGame != null)
                    return CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.CurrentGame.GameState == GameStateEnum.Started &&
                        GameModeName == CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.ActiveGameMode;
                else
                    return false;
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

                    var player = PlayerController.LocalPlayer;

                    player.RpcSetColor(ColorType.Black);
                    player.RpcSetHat(HatType.NoHat);
                    player.RpcSetSkin(SkinType.None);
                    player.RpcSetPet(PetType.NoPet);
                    player.RpcSetName("");
                });
                #endregion
            };
        }
    }
}
