using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsMod.CheepsAmongUsMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class Anon : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusanon";
        public const string PluginName = "AnonGameMode";
        public const string PluginVersion = "2.0.0";

        public const string GameModeName = "Anonymous";

        public static ManualLogSource _logger = null;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            new AnonGameMode(); // Register GameMode
        }
    }
}
