using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using CheepsAmongUsApi.API;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using CheepsAmongUsMod.API;

using PlayerControl = FFGALNAPKCD;
using MeetingHud = OOCJALPKPEP;
using DeathReasonEnum = DBLJKMDLJIF;
using ShipStatusClass = HLBNNHFCNAJ;
using GameEndReasonEnum = AIMMJPEOPEC;
using Telemetry = KOHGPBDBBJI;
using ExileController = CNNGMDOPELD;
using GameData_PlayerInfo = EGLJNOMOGNP.DCJMABDDJCF;

namespace TheJesterGameMode
{
    public class Patching
    {
        #region -------------------- Force Jester Patch --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSendChat))]
        public static class Patch_PlayerControl_RpcSendChat
        {
            internal static bool ForceJester = false;

            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] string msg)
            {
                if (TheJester.GameMode.NumJesters == 0)
                    return true;

                if (msg == "forcejester" && CheepsAmongUsMod.CheepsAmongUsMod.DecidingClient.Equals(new PlayerController(__instance)) && !GameModeManager.Selected.IsInGame)
                {
                    Notifier.ShowNotification("You will become the Jester in the next game.", 3000);
                    PlayerHudManager.HudManager.Chat.TextArea.SetText(string.Empty);

                    ForceJester = true;

                    return false;
                }

                return true;
            }
        }
        #endregion

        #region -------------------- Jester Meeting Patch --------------------
        /*[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public static class PatchMeetingHudJester
        {
            public static void Postfix(MeetingHud __instance)
            {
                if (TheJester.GameMode.NumJesters == 0)
                    return;

                if (TheJester.GameMode.AllRolePlayers.Where(x => x.AmRolePlayer).Count() == 0)
                    return;

                foreach (var obj in __instance.HBDFFAHBIGI)
                    foreach(var jester in TheJester.GameMode.AllRolePlayers)
                        if (obj.NameText.Text == jester.PlayerController.PlayerData.PlayerName)
                            obj.NameText.Color = new Color(0.74901960784f, 0, 1f);
            }
        }*/
        #endregion

        #region -------------------- Jester Being Exiled --------------------
        [HarmonyPatch(typeof(ExileController), nameof(ExileController.Begin))]
        public static class Patch_ExileController_Begin
        {
            public static void Prefix([HarmonyArgument(0)] GameData_PlayerInfo data)
            {
                if (TheJester.GameMode.NumJesters == 0 || data == null)
                    return;

                if (TheJester.GameMode.AllRolePlayers.Where(x => x.PlayerController.PlayerId == new PlayerData(data).PlayerId).Count() == 0)
                    return;

                TheJester.GameMode.JestersWon = true;
            }
        }
        #endregion

        #region -------------------- Jester Exiled --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
        public static class Patch_PlayerControl_Exiled
        {
            public static void Prefix(PlayerControl __instance)
            {
                if (TheJester.GameMode.NumJesters == 0)
                    return;

                if (TheJester.GameMode.AllRolePlayers.Where(x => x.PlayerController.Equals(new PlayerController(__instance))).Count() == 0)
                    return;

                TheJester.GameMode.JesterWon();
                RpcManager.SendRpc(TheJester.JesterRpc, (byte) TheJester.CustomRpc.JesterExiled );
            }
        }
        #endregion

        #region -------------------- Jester End Game -------------------- 
        [HarmonyPatch(typeof(Telemetry), nameof(Telemetry.EndGame))]
        public static class Patch_Telemetry_EndGame
        {
            public static void Prefix([HarmonyArgument(0)] ref GameEndReasonEnum reason)
            {
                if (TheJester.GameMode.NumJesters == 0)
                    return;

                if (TheJester.GameMode.JestersWon)
                {
                    reason = GameEndReasonEnum.ImpostorByKill;

                    foreach (var player in PlayerController.AllPlayerControls.Where(x => x.PlayerData.IsImpostor))
                        player.PlayerData.IsImpostor = false;

                    foreach (var jester in TheJester.GameMode.AllRolePlayers)
                    {
                        jester.RoleOutro.UseRoleOutro = true;
                        jester.PlayerController.PlayerData.IsImpostor = true;
                    }

                    if(TheJester.GameMode.AllRolePlayers.Where(x => x.AmRolePlayer).Count() == 0)
                    {
                        var DefeatRole = new RolePlayer(PlayerController.LocalPlayer, "Other");
                        DefeatRole.RoleOutro.WinText = "Defeat";
                        DefeatRole.RoleOutro.WinTextColor = new Color(175 / 255f, 43 / 255f, 237 / 255f);
                        DefeatRole.RoleOutro.BackgroundColor = new Color(127 / 255f, 0 / 255f, 186 / 255f);
                        DefeatRole.RoleOutro.UseRoleOutro = true;
                    }
                } else
                {
                    if(reason == GameEndReasonEnum.HumansByVote || reason == GameEndReasonEnum.HumansByTask)
                        foreach (var jester in TheJester.GameMode.AllRolePlayers)
                            jester.PlayerController.PlayerData.IsImpostor = true;
                }
            }
        }
        #endregion
    }
}
