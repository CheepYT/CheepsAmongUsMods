using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BattleRoyalGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsMod.CheepsAmongUsMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class BattleRoyale : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusbattleroyale";
        public const string PluginName = "BattleRoyaleGameMode";
        public const string PluginVersion = "2.0.0";

        public const string GameModeName = "Battle Royale";

        public const byte BattleRoyalRpc = 61;

        public static ManualLogSource _logger = null;

        public static BattleRoyaleGameMode GameMode;

        public enum CustomRpcCalls : byte
        {
            UpdateRandomStartLocation = 0,
            SendStartLocation = 1,
            SetWinner = 2,
            KillPlayer = 3,
            MurderPlayer = 4,
        }

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            #region ---------- Enable Harmony Patching ----------
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            GameMode = new BattleRoyaleGameMode();
        }
    }
}
