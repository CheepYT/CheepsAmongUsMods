using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using IntroRoutine = PENEIDJGGAF.CKACLKCOJFO;
using EjectRoutine = CNNGMDOPELD.MBGAIMHMPGN;
using OutroRoutine = ABNGEPFHMHP.EHKHLOLEFFD;

using PlayerControlClass = FFGALNAPKCD;

namespace CheepsAmongUsBaseMod
{
    public class RoleManager
    {
        public static List<RolePlayer> AllRoles = new List<RolePlayer>();

        public static bool IsLocalRolePlayer
        {
            get
            {
                return HasPlayerAnyRole(PlayerController.LocalPlayer);
            }
        }

        public static bool HasPlayerAnyRole(PlayerController player)
        {
            return AllRoles.Where(x => x.PlayerController.Equals(player)).Count() > 0;
        }

        public static void Start()
        {
            LobbyBehaviourStartedEvent.Listener += () =>
            {
                AllRoles.Clear();
            };
        }

        [HarmonyPatch(typeof(IntroRoutine), nameof(IntroRoutine.MoveNext))]
        public static class Patch_IntroRoutine_MoveNext
        {
            public static void Postfix(IntroRoutine __instance)
            {
                try
                {
                    if (!IsLocalRolePlayer)
                        return;

                    var rolePlayer = AllRoles.Where(x => x.PlayerController.IsLocalPlayer).ToList()[0];
                    var roleIntro = rolePlayer.RoleIntro;

                    roleIntro.ExecuteIntroMoveNext(rolePlayer);

                    if (!roleIntro.UseRoleIntro)
                        return;

                    var field = __instance.field_Public_PENEIDJGGAF_0;

                    __instance.isImpostor = roleIntro.IsRoleImpostor;

                    __instance.yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControlClass>();

                    foreach (var player in roleIntro.TeamPlayers)
                        __instance.yourTeam.Add(player.PlayerControl);

                    field.BackgroundBar.material.color = roleIntro.BackgroundColor;

                    field.ImpostorText.Text = roleIntro.RoleDescription;

                    field.Title.Text = roleIntro.RoleName;
                    field.Title.Color = roleIntro.RoleNameColor;
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(EjectRoutine), nameof(EjectRoutine.MoveNext))]
        public static class Patch_EjectRoutine_MoveNext
        {
            public static void Postfix(EjectRoutine __instance)
            {
                try
                {
                    var ejectedId = new PlayerData(__instance.field_Public_CNNGMDOPELD_0.LNMDIKCFBAK).PlayerController.PlayerId;
                    var available = AllRoles.Where(x => x.PlayerController.PlayerId == ejectedId);

                    if (available.Count() == 0)
                        return;

                    var rolePlayer = available.ToList()[0];
                    var roleEjected = rolePlayer.RoleEjected;

                    roleEjected.ExecuteEjectMoveNext(rolePlayer);

                    if (!roleEjected.UseRoleEjected)
                        return;

                    var field = __instance.field_Public_CNNGMDOPELD_0;

                    field.EOFFAJKKDMI = roleEjected.EjectedText;
                }
                catch { }
            }
        }

        [HarmonyPatch(typeof(OutroRoutine), nameof(OutroRoutine.MoveNext))]
        public static class Patch_OutroRoutine_MoveNext
        {
            public static void Postfix(OutroRoutine __instance)
            {
                try
                {
                    if (!IsLocalRolePlayer)
                        return;

                    var rolePlayer = AllRoles.Where(x => x.PlayerController.IsLocalPlayer).ToList()[0];
                    var roleOutro = rolePlayer.RoleOutro;

                    roleOutro.ExecuteOutroMoveNext(rolePlayer);

                    if (!roleOutro.UseRoleOutro)
                        return;

                    __instance.field_Public_ABNGEPFHMHP_0.WinText.Text = roleOutro.WinText;
                    __instance.field_Public_ABNGEPFHMHP_0.WinText.Color = roleOutro.WinTextColor;
                    __instance.field_Public_ABNGEPFHMHP_0.BackgroundBar.material.color = roleOutro.BackgroundColor;
                }
                catch { }
            }
        }

    }

    public class RolePlayer
    {
        public PlayerController PlayerController { get; }

        public RoleIntro RoleIntro { get; }

        public RoleEjected RoleEjected { get; }

        public RoleOutro RoleOutro { get; }

        public bool AmRolePlayer { get { return PlayerController.IsLocalPlayer; } }

        public RolePlayer(PlayerController player, string roleName)
        {
            PlayerController = player;

            RoleIntro = new RoleIntro();
            RoleIntro.RoleName = roleName;
            RoleIntro.TeamPlayers.Add(player);

            RoleOutro = new RoleOutro();

            RoleEjected = new RoleEjected($"{player.PlayerData.PlayerName} was " +
                $"{(GameOptions.ConfirmEjects ? $"The {RoleIntro.RoleName}" : "ejected.")}");

            RoleManager.AllRoles.Add(this);
        }
    }

    public class RoleIntro
    {
        public event EventHandler<RolePlayerEventArgs> IntroMoveNext;

        internal void ExecuteIntroMoveNext(RolePlayer rolePlayer)
        {
            IntroMoveNext?.Invoke(null, new RolePlayerEventArgs() { RolePlayer = rolePlayer });
        }

        public Color BackgroundColor { get; set; }

        public string RoleName { get; set; }

        public Color RoleNameColor { get; set; }

        public bool IsRoleImpostor { get; set; }

        public List<PlayerController> TeamPlayers { get; set; }

        public string RoleDescription { get; set; }

        public bool UseRoleIntro { get; set; }

        public RoleIntro()
        {
            BackgroundColor = new Color(194/ 255f, 249 / 255f, 255 / 255f);
            RoleNameColor = new Color(91 / 255f, 92 / 255f, 166 / 255f);

            RoleName = "Role Name";
            RoleDescription = "Role Description";

            TeamPlayers = new List<PlayerController>();

            IsRoleImpostor = false;

            UseRoleIntro = false;
        }
    }

    public class RoleEjected
    {
        public event EventHandler<RolePlayerEventArgs> EjectMoveNext;

        internal void ExecuteEjectMoveNext(RolePlayer rolePlayer)
        {
            EjectMoveNext?.Invoke(null, new RolePlayerEventArgs() { RolePlayer = rolePlayer });
        }

        public string EjectedText { get; set; }

        public bool UseRoleEjected { get; set; }

        public RoleEjected() { }
        public RoleEjected(string text) {
            EjectedText = text;
            UseRoleEjected = false;
        }
    }

    public class RoleOutro
    {
        public event EventHandler<RolePlayerEventArgs> OutroMoveNext;

        internal void ExecuteOutroMoveNext(RolePlayer rolePlayer)
        {
            OutroMoveNext?.Invoke(null, new RolePlayerEventArgs() { RolePlayer = rolePlayer });
        }

        public string WinText { get; set; }

        public Color WinTextColor { get; set; }

        public Color BackgroundColor { get; set; }

        public bool UseRoleOutro { get; set; }

        public RoleOutro() {
            WinText = "Victory";
            WinTextColor = new Color(194 / 255f, 249 / 255f, 255 / 255f);
            BackgroundColor = new Color(91 / 255f, 92 / 255f, 166 / 255f);
            UseRoleOutro = false;
        }
    }

    public class RolePlayerEventArgs : EventArgs
    {
        public RolePlayer RolePlayer { get; set; }
    }
}
