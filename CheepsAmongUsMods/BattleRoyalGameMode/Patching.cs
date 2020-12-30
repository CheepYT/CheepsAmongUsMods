using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheepsAmongUsApi.API;
using UnhollowerBaseLib;
using UnityEngine;

using PlayerControl = FFGALNAPKCD;
using GameData_PlayerInfo = EGLJNOMOGNP.DCJMABDDJCF;
using IntroClass = PENEIDJGGAF.CKACLKCOJFO;
using KillButtonClass = MLPJGKEACMM;
using GameStartManager = ANKMIOIMNFE;
using ShipStatusClass = HLBNNHFCNAJ;
using Hazel;

namespace BattleRoyal
{
    public class Patching
    {
        #region -------------------- Patch Murder Player --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class BattleRoyalPatchMurderPlayer
        {
            public static void Prefix(PlayerControl __instance)
            {
                if (!BattleRoyal.IsThisGameModeSelected)
                    return;

                new PlayerController(__instance).PlayerData.IsImpostor = true;
            }

            public static void Postfix(PlayerControl __instance)
            {
                if (!BattleRoyal.IsThisGameModeSelected)
                    return;

                new PlayerController(__instance).PlayerData.IsImpostor = false;
            }
        }
        #endregion

        #region -------------------- Patch Set Infected --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
        public static class BattleRoyalPatchSetInfected
        {
            public static void Prefix(ref Il2CppReferenceArray<GameData_PlayerInfo> JPGEIBIBJPJ)
            {
                if (!BattleRoyal.IsThisGameModeSelected)
                    return;

                JPGEIBIBJPJ = new Il2CppReferenceArray<GameData_PlayerInfo>(
                    new GameData_PlayerInfo[] { }); // Override impostors
            }
        }
        #endregion

        #region -------------------- Patch Intro --------------------
        [HarmonyPatch(typeof(IntroClass), nameof(IntroClass.MoveNext))]
        public static class BattleRoyalPatchIntroClass_MoveNext
        {
            public static void Prefix(IntroClass __instance)
            {
                if (!BattleRoyal.IsThisGameModeSelected)
                    return;

                PlayerController local = PlayerController.GetLocalPlayer();

                var team = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                team.Add(local.PlayerControl);

                __instance.isImpostor = false;
                __instance.yourTeam = team; // Team
                __instance.field_Public_PENEIDJGGAF_0.Title.Text = $"{Functions.ColorCyan}{local.PlayerData.PlayerName}";   // Title text
                __instance.field_Public_PENEIDJGGAF_0.ImpostorText.Text = "Kill everyone and be\nthe last man standing";  // Subtitle text
                __instance.field_Public_PENEIDJGGAF_0.BackgroundBar.material.color = new Color(1f, 0.8f, 0f);   // Background Bar Color
            }
        }
        #endregion

        #region -------------------- Patch Min Players --------------------
        [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
        public static class PatchMinPlayers
        {
            public static void Prefix(GameStartManager __instance)
            {
                if (!BattleRoyal.IsThisGameModeSelected)
                    return;

                __instance.MinPlayers = 2;
            }
        }
        #endregion

        #region -------------------- Patch Perform Kill --------------------
        [HarmonyPatch(typeof(KillButtonClass), nameof(KillButtonClass.PerformKill))]
        public static class PatchPerformKill
        {
            public static PlayerController Target = null;

            public static bool Prefix(KillButtonClass __instance)
            {
                var local = PlayerController.GetLocalPlayer();

                if (!BattleRoyal.IsThisGameModeActive || Target == null || (Functions.GetUnixTime() - BattleRoyal.LastKilled < GameOptions.KillCooldown && BattleRoyal.LastKilled != 0)
                    || local.PlayerData.IsDead)
                    return true;

                var target = Target;

                BattleRoyal.LastKilled = Functions.GetUnixTime();

                local.PlayerControl.MurderPlayer(target.PlayerControl);

                #region ---------- Write Kill ----------
                CheepsAmongUsBaseMod.CheepsAmongUsBaseMod.SendModCommand("KillPlayer", $"{target.NetId};{local.NetId}");
                #endregion

                return false;
            }
        }
        #endregion

        #region -------------------- Cancel End Game --------------------
        [HarmonyPatch(typeof(ShipStatusClass), nameof(ShipStatusClass.PLBGOMIEONF))] //	RpcEndGame // RVA: 0x9819F0 Offset: 0x9801F0 VA: 0x109819F0
        public static class PatchFinishEndGame
        {
            public static bool CanEndGame = false;
            public static bool Prefix()
            {
                if (!BattleRoyal.IsThisGameModeActive)
                    return true;

                return CanEndGame;
            }
        }
        #endregion

        #region -------------------- Cancel Call Meeting --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcStartMeeting))]
        public static class PatchCallMeetingEnterVentSabotage
        {
            public static bool Prefix()
            {
                if (!BattleRoyal.IsThisGameModeActive)
                    return true;

                return false;
            }
        }
        #endregion
    }
}
