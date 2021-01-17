using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using CheepsAmongUsApi.API;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

using PlayerControl = FFGALNAPKCD;
using MeetingHud = OOCJALPKPEP;
using DeathReasonEnum = DBLJKMDLJIF;
using ShipStatusClass = HLBNNHFCNAJ;
using GameEndReasonEnum = AIMMJPEOPEC;

namespace JesterGameMode
{
    public class Patching
    {
        #region -------------------- Jester Meeting Patch --------------------
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public static class PatchMeetingHudJester
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (!TheJester.IsThisGameModeActive)
                    return;

                if (!TheJester.JesterRolePlayer.AmRolePlayer)
                    return;

                foreach (var obj in __instance.HBDFFAHBIGI)
                    if (obj.NameText.Text == TheJester.JesterRolePlayer.PlayerController.PlayerData.PlayerName)
                        obj.NameText.Color = new Color(0.74901960784f, 0, 1f);
            }
        }
        #endregion

        #region -------------------- Jester Exile Patch --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
        public static class PatchJesterExile
        {
            public static void Postfix(PlayerControl __instance, DeathReasonEnum OECOPGMHMKC)
            {
                if (!TheJester.IsThisGameModeActive)
                    return;

                if (!new PlayerController(__instance).Equals(TheJester.JesterRolePlayer.PlayerController) || OECOPGMHMKC != DeathReasonEnum.Exile)
                    return;

                CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("JesterWin", $"{true}");
            }
        }
        #endregion

        #region -------------------- Jester Patch End Game --------------------
        [HarmonyPatch(typeof(ShipStatusClass), nameof(ShipStatusClass.PLBGOMIEONF))]
        public static class PatchGameEnd
        {
            internal static bool CanEndGame = false;
            public static bool Prefix(GameEndReasonEnum JMMJJGKBFJC)
            {
                if (!TheJester.IsThisGameModeActive)
                    return true;

                if (CanEndGame)
                {
                    CanEndGame = false;
                    return true;
                } else
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(2500);
                        CanEndGame = true;

                        if (TheJester.HasJesterWon)
                            ShipStatusClass.PLBGOMIEONF(GameEndReasonEnum.ImpostorByKill, false);
                        else
                            ShipStatusClass.PLBGOMIEONF(JMMJJGKBFJC, false);
                    });
                }

                return false;
            }
        }
        #endregion
    }
}
