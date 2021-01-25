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
                if (!TheJester.IsThisGameModeSelected)
                    return;

                if (!TheJester.JesterRolePlayer.AmRolePlayer)
                    return;

                foreach (var obj in __instance.HBDFFAHBIGI)
                    if (obj.NameText.Text == TheJester.JesterRolePlayer.PlayerController.PlayerData.PlayerName)
                        obj.NameText.Color = new Color(0.74901960784f, 0, 1f);
            }
        }
        #endregion

        #region -------------------- Cancel End Game --------------------
        [HarmonyPatch(typeof(ShipStatusClass), nameof(ShipStatusClass.PLBGOMIEONF))]
        public static class Patch_ShipStatusClass_RpcEndGame
        {
            internal static bool CanEndGame = false;

            public static bool Prefix(GameEndReasonEnum JMMJJGKBFJC, bool EMAKAHIFLDE)
            {
                if(!TheJester.IsThisGameModeSelected)
                    return true;

                if (!CanEndGame)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(2000);

                        if (!CanEndGame)
                        {
                            ShipStatusClass.PLBGOMIEONF(JMMJJGKBFJC, false);
                        }

                    });
                    return false;
                }


                return true;
            }
        }
        #endregion

        #region -------------------- Jester Exile Patch --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Die))]
        public static class PatchJesterExile
        {
            public static void Postfix(PlayerControl __instance, DeathReasonEnum OECOPGMHMKC)
            {
                if (!TheJester.IsThisGameModeSelected)
                    return;

                if (!new PlayerController(__instance).Equals(TheJester.JesterRolePlayer.PlayerController) || OECOPGMHMKC != DeathReasonEnum.Exile)
                    return;

                CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("JesterWon", $"{true}");
            }
        }
        #endregion
    }
}
