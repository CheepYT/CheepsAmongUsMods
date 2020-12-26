using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsBaseMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.cheep_yt.amongusapi")]
    [BepInProcess("Among Us.exe")]
    public class CheepsAmongUsBaseMod : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusbasemod";
        public const string PluginName = "CheepsAmongUsBaseMod";
        public const string PluginVersion = "1.0.0";

        public static ManualLogSource _logger = null;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");


        }

    }
}
