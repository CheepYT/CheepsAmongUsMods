using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CheepsAmongUsApi.API.Enumerations;

namespace TeleportationGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsMod.CheepsAmongUsMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class Teleportation : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusteleportation";
        public const string PluginName = "TeleportationGameMode";
        public const string PluginVersion = "2.0.0";

        public const string GameModeName = "Teleportation";

        public const byte TeleportationRpc = 68;

        public static ManualLogSource _logger = null;

        public static TeleportationGameMode GameMode;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            GameMode = new TeleportationGameMode();
        }
    }
}
