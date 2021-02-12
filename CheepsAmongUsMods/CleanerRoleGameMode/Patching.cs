using CheepsAmongUsApi.API;
using CheepsAmongUsMod.API;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeadBody = DDPGLPLGFOI;
using KillButtonManager = MLPJGKEACMM;
using PlayerControl = FFGALNAPKCD;
using ShipStatusClass = HLBNNHFCNAJ;
using GameEndReason = AIMMJPEOPEC;

namespace CleanerRoleGameMode
{
    public class Patching
    {
        #region -------------------- Clean Body --------------------
        [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.PerformKill))]
        public static class Patch_KillButtonManager_PerformKill
        {
            public static byte TargetId = byte.MaxValue;

            public static bool Prefix()
            {
                if (!Cleaner.GameMode.Enabled)
                    return true;

                if (Cleaner.GameMode.AllRolePlayers.Where(x => x.AmRolePlayer).Count() == 0)
                    return true;

                #region ----- Check Cooldown -----
                if (Cleaner.GameMode.CleanStopwatch.Elapsed.TotalSeconds < Cleaner.GameMode.CleanCooldown)
                    return false;
                #endregion

                if (TargetId != byte.MaxValue)
                {
                    Cleaner.GameMode.removeBody(TargetId);
                    RpcManager.SendRpc(Cleaner.CleanerRpc, new byte[] { (byte)CleanerGameMode.CustomRpc.CleanBody, TargetId });

                    Cleaner.GameMode.CleanStopwatch.Restart();

                    TargetId = byte.MaxValue;
                }

                return false;
            }
        }
        #endregion

        #region -------------------- Patch Set Target --------------------
        [HarmonyPatch(typeof(KillButtonManager), nameof(KillButtonManager.SetTarget))]
        public static class Patch_KillButtonManager_SetTarget
        {
            public static bool Prefix()
            {
                if (!Cleaner.GameMode.Enabled)
                    return true;

                if (Cleaner.GameMode.AllRolePlayers.Where(x => x.AmRolePlayer).Count() == 0)
                    return true;

                return false;
            }
        }
        #endregion

        #region ---------------------- Patch Exile ----------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Exiled))]
        public static class Patch_KillButtonManager_Exiled
        {
            public static void Postfix()
            {
                if (!Cleaner.GameMode.Enabled)
                    return;

                bool cleanersAlive = Cleaner.GameMode.AllRolePlayers.Where(x => !x.PlayerController.PlayerData.IsDead).Count() > 0;
                bool impostorsAlive = PlayerController.AllPlayerControls.Where(x => x.PlayerData.IsImpostor && !x.PlayerData.IsDead && Cleaner.GameMode.AllRolePlayers.Where(y => y.PlayerController.Equals(x)).Count() == 0).Count() > 0;

                if (!impostorsAlive && cleanersAlive)
                    Functions.RpcEndGame(GameEndReason.HumansByVote);
            }
        }
        #endregion

    }
}
