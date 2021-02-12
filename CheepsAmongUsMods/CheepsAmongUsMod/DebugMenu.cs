using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Hazel;
using CheepsAmongUsMod.API;
using HarmonyLib;
using UnhollowerBaseLib;

using GameEndReason = AIMMJPEOPEC;
using InnerNetClient = KHNHJFFECBP;
using AmongUsClient = FMLLKEACGIO;
using PlayerControl = FFGALNAPKCD;
using GameData_PlayerInfo = EGLJNOMOGNP.DCJMABDDJCF;
using ShipStatusClass = HLBNNHFCNAJ;

namespace CheepsAmongUsMod
{
    public class DebugMenu
    {
        private static KeyCode[] OpenCloseDebug = { KeyCode.LeftShift, KeyCode.LeftControl, KeyCode.LeftAlt, KeyCode.D };

        private static bool IsDebugOpen = false;

        private static List<CustomText> Texts = new List<CustomText>();

        private static List<CustomText> SelectImpostorTexts = new List<CustomText>();

        private static List<byte> SelectedImpostorIds = new List<byte>();

        private const float Distance = 0.3f;

        public static bool RandomizeImpostors = true;

        public static bool ImpostorSelectorOpen = false;

        public static bool CancelDefaultEndGame = false;

        private static void CloseDebug()
        {
            CheepsAmongUsMod._logger.LogInfo("Closing debug menu");

            foreach (var text in Texts)
                text.Destroy();
            Texts.Clear();

            foreach (var text in SelectImpostorTexts)
                text.Destroy();
            SelectImpostorTexts.Clear();

            IsDebugOpen = false;
            ImpostorSelectorOpen = false;
        }

        private static CustomText CreateText(float x, float y, string text)
        {
            var ctext = new CustomText(x, y, text);
            ctext.TextRenderer.Centered = true;

            return ctext;
        }

        private static void OpenDebug()
        {
            CheepsAmongUsMod._logger.LogInfo("Opening debug menu");

            Texts.Add(CreateText(0f, 2.5f, "Position: "));

            Texts.Add(CreateText(-2.3f, 2f, "Force End Game"));
            Texts.Add(CreateText(0f, 2f, "Disable Collision"));
            Texts.Add(CreateText(2.3f, 2f, PlayerController.LocalPlayer.PlayerData.IsImpostor ? "Unset Impostor" : "Set Impostor"));

            Texts.Add(CreateText(-2.3f, 1.5f, PlayerController.LocalPlayer.PlayerData.IsDead ? "Set Alive" : "Set Dead"));

            if(!GameModeManager.Selected.IsInGame)
                Texts.Add(CreateText(0f, 1.5f, "Select Impostors"));

            Texts.Add(CreateText(2.3f, 1.5f, "Disable ShadowQuad"));

            if(CancelDefaultEndGame)
                Texts.Add(CreateText(0f, 1f, "Disable Cancel End Game"));
            else
                Texts.Add(CreateText(0f, 1f, "Enable Cancel End Game"));

            Texts.Add(CreateText(0f, -2.5f, "Close Menu"));

            IsDebugOpen = true;
        }

        public static void Start()
        {
            CheepsAmongUsMod._logger.LogInfo("Started DebugMenu");

            HudUpdateEvent.Listener += () =>
            {
                bool openCloseDebug = true;

                for (int i = 0; i < OpenCloseDebug.Length; i++)
                    if (i == OpenCloseDebug.Length - 1)
                        openCloseDebug &= Functions.GetKeyDown(OpenCloseDebug[i]);
                    else
                        openCloseDebug &= Functions.GetKey(OpenCloseDebug[i]);

                if (openCloseDebug)
                {
                    if (IsDebugOpen)
                        CloseDebug();
                    else
                        OpenDebug();
                }

                if (ImpostorSelectorOpen)
                {
                    foreach(var text in SelectImpostorTexts)
                    {
                        try
                        {
                            bool clicked = Functions.GetKeyDown(KeyCode.Mouse0) && Vector2.Distance(text.gameObject.transform.position, Functions.MouseWorldPosition) < Distance;

                            if(text.Text == "Random Impostors")
                            {
                                if (clicked)
                                {
                                    SelectedImpostorIds.Clear();
                                    continue;
                                }

                                if (SelectedImpostorIds.Count == 0)
                                    text.Color = Color.red;
                                else
                                    text.Color = Color.white;
                            }

                            if(text.Text == "Close Menu!" && clicked)
                            {
                                CloseDebug();
                                return;
                            }

                            var pId = PlayerController.FromName(text.Text).PlayerId;

                            if (SelectedImpostorIds.Contains(pId))
                            {
                                text.Color = Color.red;

                                if (clicked)
                                    SelectedImpostorIds.Remove(pId);
                            } else
                            {
                                text.Color = Color.white;

                                if (clicked)
                                    SelectedImpostorIds.Add(pId);
                            }
                        }
                        catch { }
                    }
                }

                if (IsDebugOpen) { 
                    var posText = Texts.Where(x => x.Text.StartsWith("Position: ")).First();

                    var pos = PlayerController.LocalPlayer.GameObject.transform.position;

                    posText.Text = $"Position: {pos.x}, {pos.y}, {pos.z}";

                    if (Functions.GetKeyDown(KeyCode.Mouse0))
                    {
                        foreach (var text in Texts)
                            if (Vector2.Distance(text.gameObject.transform.position, Functions.MouseWorldPosition) < Distance)
                            {
                                switch (text.Text)
                                {
                                    case "Force End Game":
                                        {
                                            CheepsAmongUsMod._logger.LogInfo("Force Ending Game...");

                                            CloseDebug();

                                            MessageWriter messageWriter = AmongUsClient.Instance.StartEndGame();
                                            messageWriter.Write((byte)GameEndReason.ImpostorByKill);
                                            messageWriter.Write(false);
                                            AmongUsClient.Instance.FinishEndGame(messageWriter);
                                            return;
                                        }

                                    case "Disable ShadowQuad":
                                    case "Enable ShadowQuad":
                                        {
                                            var enabled = text.Text != "Disable ShadowQuad";

                                            if (enabled)
                                                text.Text = "Disable ShadowQuad";
                                            else
                                                text.Text = "Enable ShadowQuad";

                                            CheepsAmongUsMod._logger.LogInfo("Setting ShadowQuad to " + enabled);

                                            PlayerHudManager.HudManager.ShadowQuad.gameObject.SetActive(enabled);
                                            break;
                                        }

                                    case "Enable Collision":
                                    case "Disable Collision":
                                        {
                                            var collision = !PlayerController.LocalPlayer.HasCollision;

                                            if (collision)
                                                text.Text = "Disable Collision";
                                            else
                                                text.Text = "Enable Collision";

                                            CheepsAmongUsMod._logger.LogInfo("Setting collision to " + collision);

                                            PlayerController.LocalPlayer.HasCollision = collision;
                                            break;
                                        }

                                    case "Set Impostor":
                                    case "Unset Impostor":
                                        {
                                            var impostor = !PlayerController.LocalPlayer.PlayerData.IsImpostor;

                                            CheepsAmongUsMod._logger.LogInfo("Setting impostor to " + impostor);

                                            if (impostor)
                                                text.Text = "Unset Impostor";
                                            else
                                                text.Text = "Set Impostor";

                                            PlayerController.LocalPlayer.PlayerData.IsImpostor = impostor;

                                            break;
                                        }

                                    case "Enable Cancel End Game":
                                    case "Disable Cancel End Game":
                                        {
                                            CancelDefaultEndGame = !CancelDefaultEndGame;

                                            if (CancelDefaultEndGame)
                                                text.Text = "Disable Cancel End Game";
                                            else
                                                text.Text = "Enable Cancel End Game";

                                            break;
                                        }

                                    case "Select Impostors":
                                        {
                                            CloseDebug();

                                            ImpostorSelectorOpen = true;

                                            var rnd = CreateText(0f, 2.5f, "Random Impostors");
                                            rnd.Color = Color.red;
                                            SelectImpostorTexts.Add(rnd);

                                            SelectImpostorTexts.Add(CreateText(0f, -2.5f, "Close Menu!"));

                                            float currentX = -2.3f;
                                            float currentY = 2f;

                                            foreach (var player in PlayerController.AllPlayerControls) {
                                                SelectImpostorTexts.Add(CreateText(currentX, currentY, player.PlayerData.PlayerName));

                                                currentX += 2.3f;

                                                if (currentX > 2.3f)
                                                {
                                                    currentX = -2.3f;
                                                    currentY -= 0.5f;
                                                }
                                            }

                                            break;
                                        }

                                    case "Set Dead":
                                    case "Set Alive":
                                        {
                                            var dead = !PlayerController.LocalPlayer.PlayerData.IsDead;

                                            CheepsAmongUsMod._logger.LogInfo("Setting dead to " + dead);

                                            if (dead)
                                                text.Text = "Set Alive";
                                            else
                                                text.Text = "Set Dead";

                                            PlayerController.LocalPlayer.PlayerData.IsDead = dead;

                                            break;
                                        }

                                    case "Close Menu":
                                        {
                                            CloseDebug();
                                            return;
                                        }
                                }
                            }
                    }
                }
            };

            LobbyBehaviourStartedEvent.Listener += () =>
            {
                SelectedImpostorIds.Clear();
            };
        }
    
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
        public static class Patch_PlayerControl_RpcSetInfected
        {
            public static void Prefix([HarmonyArgument(0)] ref Il2CppReferenceArray<GameData_PlayerInfo> infected)
            {
                if (SelectedImpostorIds.Count == 0)
                    return;

                var impostors = new List<GameData_PlayerInfo>();

                foreach (var imp in SelectedImpostorIds)
                    try
                    {
                        impostors.Add(PlayerController.FromPlayerId(imp).PlayerData.PlayerDataObject);
                    }
                    catch { }

                infected = impostors.ToArray();
            }
        }

        [HarmonyPatch(typeof(ShipStatusClass), nameof(ShipStatusClass.PLBGOMIEONF))]
        public static class Patch_ShipStatusClass_RpcEndGame
        {
            public static bool Prefix()
            {
                return !CancelDefaultEndGame;
            }
        }
    }
}
