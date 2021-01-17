using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using System;
using UnityEngine;
using PowerTools;
using System.Collections.Generic;
using BepInEx.Configuration;
using System.Threading.Tasks;
using CheepsAmongUsApi.API;

namespace CheepsAmongUsApi
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInProcess("Among Us.exe")]
    public class CheepsAmongUs : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusapi";
        public const string PluginName = "CheepsAmongUsApi";
        public const string PluginVersion = "1.1.54";

        public static ManualLogSource _logger = null;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo("Loading plugin API " + PluginName);

            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            CustomButton.StartManager();
        }
    }
}
