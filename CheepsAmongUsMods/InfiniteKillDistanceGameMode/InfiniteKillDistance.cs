using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheepsAmongUsBaseMod;
using CheepsAmongUsApi.API;
using HarmonyLib;
using System.Reflection;
using CheepsAmongUsApi.API.Events;

using GameOptions = KMOGFLPJLLK;
using OptionBehaviour = LLKOLCLGCBD;
using GameOptionsMenu = PHCKLDDNJNP;
using StringOption = PHHPFHDMAGH;

namespace InfiniteKillDistanceGameMode
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]    // Add the API as a dependency
    [BepInDependency(CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.PluginGuid)] // Add the base mod as dependency
    [BepInProcess("Among Us.exe")]
    public class InfiniteKillDistance : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusinfinitekilldistance";
        public const string PluginName = "InfiniteKillDistanceAddOn";
        public const string PluginVersion = "1.0.3";

        public const string InfiniteName = "Infinite";

        public const float InfiniteDistance = float.MaxValue;

        public static ManualLogSource _logger = null;

        private static float[] GameKillDistances
        {
            get
            {
                return GameOptions.JMLGACIOLIK; // 0x4
            }

            set
            {
                GameOptions.JMLGACIOLIK = value; // 0x4
            }
        }
        private static string[] GameKillDistanceNames
        {
            get
            {
                return GameOptions.BCMNACENFIL; // 0x8
            }

            set
            {
                GameOptions.BCMNACENFIL = value; // 0x8
            }
        }

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            #region ---------- Enable Harmony Patching ----------
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            #region ---------- Add to Default Values ----------
            List<float> KillDistances = new List<float>(GameKillDistances); 
            List<string> KillDistanceNames = new List<string>(GameKillDistanceNames); 

            KillDistances.Add(InfiniteDistance);
            KillDistanceNames.Add(InfiniteName);

            GameKillDistances = KillDistances.ToArray();
            GameKillDistanceNames = KillDistanceNames.ToArray();
            #endregion

            #region ---------- Replace Distances on Game Start ----------
            GameStartedEvent.Listener += () =>
            {
                if (CheepsAmongUsApi.API.GameOptions.KillDistanceValue == KillDistances.Count - 1)
                {
                    for (int i = 0; i < GameKillDistances.Length; i++)
                        GameKillDistances[i] = InfiniteDistance;
                }
                else
                {
                    GameKillDistances = KillDistances.ToArray();
                }
            };
            #endregion
        }

        #region -------------------- Patch Game Options Menu --------------------
        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Start))]
        public static class Patch_GameOptionsMenu_Start
        {
            public static void Postfix()
            {
                foreach (var obj in UnityEngine.Object.FindObjectsOfType<OptionBehaviour>())
                    if (obj.Title == StringNames.GameKillDistance)
                    {
                        StringOption option = new StringOption(obj.Pointer);

                        List<StringNames> StrNames = new List<StringNames>(option.Values);
                        StrNames.Add(StringNames.Accept);

                        option.Values = StrNames.ToArray();
                    }
            }
        }

        [HarmonyPatch(typeof(GameOptionsMenu), nameof(GameOptionsMenu.Update))]
        public static class Patch_GameOptionsMenu_Update
        {
            public static void Postfix()
            {
                foreach (var obj in UnityEngine.Object.FindObjectsOfType<OptionBehaviour>())
                {
                    if (obj.Title == StringNames.GameKillDistance)
                    {
                        StringOption option = new StringOption(obj.Pointer);

                        if (option.Values[option.Value] == StringNames.Accept)
                            option.ValueText.Text = InfiniteName;
                    }
                }
            }
        }
        #endregion
    }
}
