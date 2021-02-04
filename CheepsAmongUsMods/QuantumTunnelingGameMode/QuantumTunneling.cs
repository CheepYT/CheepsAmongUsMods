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

namespace QuantumTunnelingGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsMod.CheepsAmongUsMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class QuantumTunneling : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusquantumtunneling";
        public const string PluginName = "QuantumTunnelingGameMode";
        public const string PluginVersion = "2.0.0";

        public const byte QuantumTunnelingRpc = 66;

        public const string Delimiter = "\n----------\n";

        public static ManualLogSource _logger = null;

        public QuantumTunnelingGameMode GameMode;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            GameMode = new QuantumTunnelingGameMode(Config);
        }
    }
}
