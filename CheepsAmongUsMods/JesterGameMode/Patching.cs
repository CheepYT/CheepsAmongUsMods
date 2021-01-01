using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using CheepsAmongUsApi.API;
using UnityEngine;
using System.Threading.Tasks;

using IntroClass = PENEIDJGGAF.CKACLKCOJFO;
using PlayerControl = FFGALNAPKCD;
using MeetingHud = OOCJALPKPEP;
using GameEndReason = AIMMJPEOPEC;
using ShipStatusClass = HLBNNHFCNAJ;
using TextRendererClass = AELDHKGBIFD;

namespace JesterGameMode
{
    public class Patching
    {
        #region -------------------- Jester Intro Patch --------------------
        [HarmonyPatch(typeof(IntroClass), nameof(IntroClass.MoveNext))]
        public static class PatchIntroClass_MoveNext
        {
            public static void Postfix(IntroClass __instance)
            {
                if (!Jester.IsThisGameModeSelected)
                    return;

                if (!Jester.TheJester.AmPlayerController())
                    return;

                PlayerController local = PlayerController.GetLocalPlayer();

                var team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                team.Add(local.PlayerControl);

                __instance.yourTeam = team; // Team

                __instance.field_Public_PENEIDJGGAF_0.Title.Text = $"{Functions.ColorPurple}The Jester";

                __instance.field_Public_PENEIDJGGAF_0.ImpostorText.Text =
                                $"Trick the crewmates into thinking\n" +
                                $"that you are an {Functions.ColorRed}Impostor[]\n";

                __instance.field_Public_PENEIDJGGAF_0.BackgroundBar.material.color = new Color(0.74901960784f, 0, 1f);
            }
        }
        #endregion

        #region -------------------- Jester Meeting Patch --------------------
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public static class PatchMeetingHudJester
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (!Jester.IsThisGameModeActive)
                    return;

                if (!Jester.TheJester.AmPlayerController())
                    return;

                foreach (var obj in __instance.HBDFFAHBIGI)
                    if (obj.NameText.Text == Jester.TheJester.PlayerData.PlayerName)
                        obj.NameText.Color = new Color(0.74901960784f, 0, 1f);
            }
        }
        #endregion

        #region -------------------- Jester Exile Patch --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
        public static class PatchJesterExile
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (!Jester.IsThisGameModeActive)
                    return;

                if (!Jester.TheJester.AmPlayerController() || !new PlayerController(__instance).Equals(Jester.TheJester))
                    return;

                // Player exiled is the jester
                CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("JesterWin", $"{true}"); // Call Jester Win
            }
        }
        #endregion

        #region -------------------- Patch Rpc End Game --------------------
        [HarmonyPatch(typeof(ShipStatusClass), nameof(ShipStatusClass.PLBGOMIEONF))] //	RpcEndGame // RVA: 0x9819F0 Offset: 0x9801F0 VA: 0x109819F0
        public static class PatchFinishEndGame
        {
            public static bool CanEndGame = false;
            public static bool Prefix(GameEndReason JMMJJGKBFJC)
            {
                if (!Jester.IsThisGameModeActive)
                    return true;

                if (JMMJJGKBFJC == GameEndReason.HumansByTask)
                    Jester.TasksCompleted = true;

                return CanEndGame;
            }
        }
        #endregion

    }
}
