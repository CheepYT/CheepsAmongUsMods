using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheepsAmongUsApi.API;
using UnityEngine;
using UnhollowerBaseLib;

using PlayerControl = FFGALNAPKCD;
using GameData_PlayerInfo = EGLJNOMOGNP.DCJMABDDJCF;
using IntroClass = PENEIDJGGAF.CKACLKCOJFO;
using KillButtonClass = MLPJGKEACMM;
using GameStartManager = ANKMIOIMNFE;
using ShipStatusClass = HLBNNHFCNAJ;
using PlayerTab = MAOILGPNFND;
using Palette = LOCPGOACAJF;
using GameData = EGLJNOMOGNP;
using DeadBody = DDPGLPLGFOI;
using SystemConsole = IMPCIAEIBNB;

namespace BattleRoyale
{
    public class Patching
    {
        #region -------------------- Patch Murder Player --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class BattleRoyalPatchMurderPlayer
        {
            public static void Prefix(PlayerControl __instance)
            {
                if (!BattleRoyale.IsThisGameModeSelected)
                    return;

                PlayerController instance = new PlayerController(__instance);
                instance.PlayerData.IsImpostor = true;
            }

            public static void Postfix(PlayerControl __instance)
            {
                if (!BattleRoyale.IsThisGameModeSelected)
                    return;

                PlayerController instance = new PlayerController(__instance);
                instance.PlayerData.IsImpostor = false;
            }
        }
        #endregion

        #region -------------------- Patch Set Infected --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
        public static class BattleRoyalPatchSetInfected
        {
            public static void Prefix(ref Il2CppReferenceArray<GameData_PlayerInfo> JPGEIBIBJPJ)
            {
                if (!BattleRoyale.IsThisGameModeSelected)
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
                if (!BattleRoyale.IsThisGameModeSelected)
                    return;

                PlayerController local = PlayerController.LocalPlayer;

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
                if (!BattleRoyale.IsThisGameModeSelected)
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
                var local = PlayerController.LocalPlayer;

                if (!BattleRoyale.IsThisGameModeActive || Target == null || (Functions.GetUnixTime() - BattleRoyale.LastKilled < GameOptions.KillCooldown && BattleRoyale.LastKilled != 0)
                    || local.PlayerData.IsDead)
                    return true;

                var target = Target;

                BattleRoyale.LastKilled = Functions.GetUnixTime();

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
                if (!BattleRoyale.IsThisGameModeActive)
                    return true;

                return CanEndGame;
            }
        }
        #endregion

        /*
        #region ------------------------------ Patch Player Limit ------------------------------
        [HarmonyPatch(typeof(GameData), nameof(GameData.GetAvailableId))]
        public static class GameDataAvailableIdPatch
        {
            public static bool Prefix(ref GameData __instance, ref sbyte __result)
            {
                if (!BattleRoyale.RemovePlayerLimit || !BattleRoyale.IsThisGameModeSelected)
                    return true;

                for (int i = 0; i < 128; i++)
                    if (checkId(__instance, i))
                    {
                        __result = (sbyte)i;
                        return false;
                    }
                __result = -1;
                return false;
            }

            static bool checkId(GameData __instance, int id)
            {
                foreach (var p in __instance.AllPlayers)
                    if (p.JKOMCOJCAID == id)
                        return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckColor), typeof(byte))]
        public static class PlayerControlCheckColorPatch
        {
            public static bool Prefix(PlayerControl __instance, byte POCIJABNOLE)
            {
                if (!BattleRoyale.RemovePlayerLimit || !BattleRoyale.IsThisGameModeSelected)
                    return true;

                __instance.RpcSetColor(POCIJABNOLE);
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerTab), nameof(PlayerTab.UpdateAvailableColors))]
        public static class PlayerTabUpdateAvailableColorsPatch
        {
            public static bool Prefix(PlayerTab __instance)
            {
                if (!BattleRoyale.RemovePlayerLimit || !BattleRoyale.IsThisGameModeSelected)
                    return true;

                PlayerControl.SetPlayerMaterialColors(PlayerControl.LocalPlayer.NDGFFHMFGIG.EHAHBDFODKC, __instance.DemoImage);
                for (int i = 0; i < Palette.OPKIKLENHFA.Length; i++)
                    __instance.LGAIKONLBIG.Add(i);
                return false;
            }
        }
        #endregion
        */

        #region -------------------- Cancel Do Click Report/Use --------------------
        [HarmonyPatch(typeof(DeadBody), nameof(DeadBody.OnClick))]
        public static class Patch_DeadBody_OnClick
        {
            public static bool Prefix()
            {
                if (!BattleRoyale.IsThisGameModeSelected)
                    return true;

                return false;
            }
        }

        [HarmonyPatch(typeof(SystemConsole), nameof(SystemConsole.Use))]
        public static class Patch_SystemConsole_Use
        {
            public static bool Prefix()
            {
                if (!BattleRoyale.IsThisGameModeSelected)
                    return true;

                return false;
            }
        }
        #endregion
    }
}
