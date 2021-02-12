using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsMod.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CleanerRoleGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsMod.CheepsAmongUsMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class Cleaner : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.cleaner";
        public const string PluginName = "CleanerGameMode";
        public const string PluginVersion = "1.0.0";

        public const string GameModeName = "Cleaner";

        internal static ManualLogSource _logger = null;

        public const byte CleanerRpc = 72;

        public static CleanerGameMode GameMode;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            #region ---------- Enable Harmony Patching ----------
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            GameMode = new CleanerGameMode();
        }
    }
}
