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
using MeetingHud = OOCJALPKPEP;
using PlayerControlClass = FFGALNAPKCD;

namespace CheepsAmongUsMod.API
{
    public class RoleManager
    {
        public static List<RolePlayer> AllRoles = new List<RolePlayer>();

        /// <summary>
        /// Returns true, if the local player has any role
        /// </summary>
        public static bool IsLocalRolePlayer
        {
            get
            {
                return HasPlayerAnyRole(PlayerController.LocalPlayer);
            }
        }

        /// <summary>
        /// Checks if a given player has a role
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>True, if the player has any role</returns>
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

            GameStartedEvent.Listener += () =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(2500);

                    foreach(var role in AllRoles)
                        if (role.NameColorVisible)
                            role.PlayerController.PlayerControl.nameText.Color = role.NameColor;
                });
            };
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
        public static class PatchMeetingHudJester
        {
            public static void Postfix(MeetingHud __instance)
            {
                foreach (var obj in __instance.HBDFFAHBIGI)
                    foreach (var role in AllRoles)
                        if (role.NameColorVisible && obj.NameText.Text == role.PlayerController.PlayerData.PlayerName)
                            role.PlayerController.PlayerControl.nameText.Color = role.NameColor;
            }
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

                    field.ImpostorText.gameObject.SetActive(true);

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
        /// <summary>
        /// PlayerController that has the specific role
        /// </summary>
        public PlayerController PlayerController { get; }

        /// <summary>
        /// RoleIntro instance
        /// </summary>
        public RoleIntro RoleIntro { get; }

        /// <summary>
        /// RoleEjected instance
        /// </summary>
        public RoleEjected RoleEjected { get; }

        /// <summary>
        /// Role outro instance
        /// </summary>
        public RoleOutro RoleOutro { get; }

        /// <summary>
        /// True, if the local player has the specific role
        /// </summary>
        [Obsolete("Moved to IsRolePlayer")]
        public bool AmRolePlayer { get { return PlayerController.IsLocalPlayer; } }

        /// <summary>
        /// True, if the local player has the specific role
        /// </summary>
        public bool IsRolePlayer { get { return PlayerController.IsLocalPlayer; } }

        /// <summary>
        /// The color of the players name (Game and in Meetings)
        /// </summary>
        public Color NameColor { get; set; }

        /// <summary>
        /// If this is true, the players name color will assume the "NameColor" value
        /// </summary>
        public bool NameColorVisible { get; set; }

        /// <summary>
        /// Creates and registeres a new instance of a RolePlayer
        /// </summary>
        /// <param name="player">The PlayerController that has the role</param>
        /// <param name="roleName">The name of the role. If RoleIntro and RoleEjected are not overridden, this name will be used</param>
        public RolePlayer(PlayerController player, string roleName)
        {
            PlayerController = player;

            RoleIntro = new RoleIntro
            {
                RoleName = roleName
            };

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
