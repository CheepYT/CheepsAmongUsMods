using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#region -------------------- Among Us Types --------------------
using HudManagerClass = PIEFJFEOGOL;
using GameJoinClass = KHNHJFFECBP;
using GameStartClass = HLBNNHFCNAJ;
using GameExitClass = FMLLKEACGIO;
using PlayerControlClass = FFGALNAPKCD;
using MainMenuClass = BOCOFLHKCOJ;
using PingClass = ELDIDNABIPI;
using TextRendererClass = AELDHKGBIFD;
#endregion

namespace CheepsAmongUsApi.API.Events
{
    /// <summary>
    /// Notify delegate
    /// </summary>
    public delegate void Notify();

    /// <summary>
    /// The class to specific in game call events
    /// </summary>
    public class EventPatches
    {
        #region -------------------- Patch Game Started Event --------------------
        [HarmonyPatch(typeof(GameStartClass), nameof(GameStartClass.Start))]
        public static class Patch_GameStartClass_Start
        {
            public static void Postfix()
            {
                try
                {
                    GameStartedEvent.CallEvent();
                }
                catch (Exception e)
                {
                    CheepsAmongUs._logger.LogError("Error invoking GameStartedEvent: " + e);
                }
            }
        }
        #endregion

        #region -------------------- Patch Hud Update Event --------------------
        [HarmonyPatch(typeof(HudManagerClass), nameof(HudManagerClass.Update))]
        public static class Patch_HudManagerClass_Update
        {
            public static void Postfix()
            {
                try
                {
                    HudUpdateEvent.CallEvent();
                }
                catch (Exception e)
                {
                    CheepsAmongUs._logger.LogError("Error invoking HudUpdateEvent: " + e);
                }
            }
        }
        #endregion

        #region -------------------- Patch Game Joined Event --------------------
        [HarmonyPatch(typeof(GameJoinClass), nameof(GameJoinClass.JoinGame))]
        public static class Patch_GameJoinClass_JoinGame
        {
            public static void Postfix()
            {
                try
                {
                    JoinedLobbyEvent.CallEvent();
                }
                catch (Exception e)
                {
                    CheepsAmongUs._logger.LogError("Error invoking JoinedLobbyEvent: " + e);
                }
            }
        }
        #endregion

        #region -------------------- Patch Player Update Event --------------------
        [HarmonyPatch(typeof(PlayerControlClass), nameof(PlayerControlClass.FixedUpdate))]
        public static class Patch_PlayerControlClass_FixedUpdate
        {
            public static void Postfix()
            {
                try
                {
                    PlayerUpdateEvent.CallEvent();
                }
                catch (Exception e)
                {
                    CheepsAmongUs._logger.LogError("Error invoking PlayerUpdateEvent: " + e);
                }
            }
        }
        #endregion

        #region -------------------- Patch Player Exit Event --------------------
        [HarmonyPatch(typeof(GameExitClass), nameof(GameExitClass.ExitGame))]
        public static class Patch_GameExitClass_ExitGame
        {
            public static void Postfix()
            {
                try
                {
                    ExitLobbyEvent.CallEvent();
                }
                catch (Exception e)
                {
                    CheepsAmongUs._logger.LogError("Error invoking ExitLobbyEvent: " + e);
                }
            }
        }
        #endregion

        #region -------------------- Patch Version Text --------------------
        [HarmonyPatch(typeof(MainMenuClass), nameof(MainMenuClass.Start))]
        public static class Patch_MainMenuClass_Start
        {
            internal static string TextToAppend = string.Empty;
            internal static bool IsCentered = false;

            public static void Postfix(MainMenuClass __instance)
            {
                __instance.text.Centered = IsCentered;
                __instance.text.Text += TextToAppend;  //BOCOFLHKCOJ or IJCMADIPDHJ
            }
        }
        #endregion

        #region -------------------- Patch Ping Text --------------------
        [HarmonyPatch(typeof(PingClass), nameof(PingClass.Update))]
        public static class Patch_PingClass_Update
        {
            internal static string TextToAppend = string.Empty;
            internal static bool IsCentered = false;

            public static void Postfix(PingClass __instance)
            {
                __instance.text.Centered = IsCentered;
                __instance.text.Text += TextToAppend;  //BOCOFLHKCOJ or IJCMADIPDHJ
            }
        }
        #endregion

        #region -------------------- Patch Intro Texts --------------------
        [HarmonyPatch(typeof(TextRendererClass), nameof(TextRendererClass.Start))]
        public static class PatchTextRendererStart
        {
            internal static string CrewmateText = string.Empty;
            internal static string DescriptionText = string.Empty;
            internal static string DescriptionTextBefore = string.Empty;
            internal static string DescriptionTextAfter = string.Empty;
            internal static string ImpostorText = string.Empty;
            internal static string VictoryText = string.Empty;
            internal static string DefeatText = string.Empty;

            public static void Postfix(TextRendererClass __instance)
            {
                if (__instance.Text == "Crewmate" && CrewmateText != string.Empty)
                {
                    __instance.Text = CrewmateText;
                    CrewmateText = string.Empty;
                }

                if (__instance.Text == "Impostor" && ImpostorText != string.Empty)
                {
                    __instance.Text = ImpostorText;
                    ImpostorText = string.Empty;
                }

                if (__instance.Text.Contains("[] among us") && DescriptionTextBefore != string.Empty)
                {
                    __instance.Text = DescriptionTextBefore + __instance.Text;
                    DescriptionTextBefore = string.Empty;
                }

                if (__instance.Text.Contains("[] among us") && DescriptionTextAfter != string.Empty)
                {
                    __instance.Text += DescriptionTextAfter;
                    DescriptionTextAfter = string.Empty;
                }

                if (__instance.Text.Contains("Victory") && VictoryText != string.Empty)
                {
                    __instance.Text = VictoryText;
                    VictoryText = string.Empty;
                }

                if (__instance.Text.Contains("Defeat") && DefeatText != string.Empty)
                {
                    __instance.Text = DefeatText;
                    DefeatText = string.Empty;
                }

                if (__instance.Text.Contains("[] among us") && DescriptionText != string.Empty)
                {
                    __instance.Text = DescriptionText;
                    DescriptionText = string.Empty;
                }
            }
        }
        #endregion
    }
}
