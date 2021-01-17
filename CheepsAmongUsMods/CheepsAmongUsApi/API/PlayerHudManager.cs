using CheepsAmongUsApi.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#region -------------------- Among Us Types --------------------
using HudManagerClass = PIEFJFEOGOL; // TypeDefIndex: 7254
using HudManagerObj = PPAEIPHJPDH<PIEFJFEOGOL>; // TypeDefIndex: 7254
using TextRendererObject = AELDHKGBIFD;
#endregion

namespace CheepsAmongUsApi.API
{
    public class PlayerHudManager
    {
        public static HudManagerClass HudManager { get { return HudManagerObj.IAINKLDJAGC; } }  // TypeDefIndex: 7054  // 0x0

        public static PlayerController PlayerController = PlayerController.LocalPlayer;

        [Obsolete]
        public static void SetCrewmateIntroText(string text)
        {
            EventPatches.PatchTextRendererStart.CrewmateText = text;
        }

        [Obsolete]
        public static void SetImpostorIntroText(string text)
        {
            EventPatches.PatchTextRendererStart.ImpostorText = text;
        }

        [Obsolete]
        public static void SetCrewmateSubtitleText(string text)
        {
            EventPatches.PatchTextRendererStart.DescriptionText = text;
        }

        [Obsolete]
        public static void SetCrewmateSubtitleTextBefore(string text)
        {
            EventPatches.PatchTextRendererStart.DescriptionTextBefore = text;
        }

        [Obsolete]
        public static void SetCrewmateSubtitleTextAfter(string text)
        {
            EventPatches.PatchTextRendererStart.DescriptionTextAfter = text;
        }

        [Obsolete]
        public static void SetVictoryText(string text)
        {
            EventPatches.PatchTextRendererStart.VictoryText = text;
        }

        [Obsolete]
        public static void SetDefeatText(string text)
        {
            EventPatches.PatchTextRendererStart.DefeatText = text;
        }

        public static bool IsChatVisible
        {
            set
            {
                HudManager.Chat.SetVisible(value);
            }
        }

        public static void AddChat(PlayerController player, string message)
        {
            HudManager.Chat.AddChat(player.PlayerControl, message);
        }

        public static string AppendedVersionText
        {
            get
            {
                return EventPatches.Patch_MainMenuClass_Start.TextToAppend;
            }

            set
            {
                EventPatches.Patch_MainMenuClass_Start.TextToAppend = value;
            }
        }

        public static bool UseAppendedVersionText = true;

        public static bool IsVersionTextCentered
        {
            get
            {
                return EventPatches.Patch_MainMenuClass_Start.IsCentered;
            }

            set
            {
                EventPatches.Patch_MainMenuClass_Start.IsCentered = value;
            }
        }

        public static string AppendedPingText
        {
            get
            {
                return EventPatches.Patch_PingClass_Update.TextToAppend;
            }

            set
            {
                EventPatches.Patch_PingClass_Update.TextToAppend = value;
            }
        }

        public static bool UseAppendedAppendedPingText = true;

        public static bool IsPingTextCentered
        {
            get
            {
                return EventPatches.Patch_PingClass_Update.IsCentered;
            }

            set
            {
                EventPatches.Patch_PingClass_Update.IsCentered = value;
            }
        }

        public static string TaskText
        {
            get
            {
                return HudManager.TaskText.Text;
            }
            set
            {
                HudManager.TaskText.Text = value;
            }
        }

        public static string GameSettingsText
        {
            get
            {
                return HudManager.GameSettings.Text;
            }
            set
            {
                HudManager.GameSettings.Text = value;
            }
        }
    }
}
