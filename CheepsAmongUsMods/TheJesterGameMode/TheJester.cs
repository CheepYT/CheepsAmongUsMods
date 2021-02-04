using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;
using ShipStatusClass = HLBNNHFCNAJ;
using GameEndReasonEnum = AIMMJPEOPEC;
using CheepsAmongUsMod.API;
using CheepsAmongUsApi.API.CustomGameOptions;

namespace TheJesterGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsMod.CheepsAmongUsMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class TheJester : BasePlugin
    {
        public enum CustomRpc : byte
        {
            UpdateJesterCount = 0,
            SetJester = 1,
            JesterExiled = 2
        }

        public const string PluginGuid = "com.cheep_yt.amongusjester";
        public const string PluginName = "JesterGameMode";
        public const string PluginVersion = "2.0.0";

        public const string GameModeName = "Jester";

        public const byte JesterRpc = 67;

        internal static ManualLogSource _logger = null;

        public static TheJesterRoleGameMode GameMode;
        
        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            #region ---------- Enable Harmony Patching ----------
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            GameMode = new TheJesterRoleGameMode(Config); // Create Role GameMode
        }
    }
}
