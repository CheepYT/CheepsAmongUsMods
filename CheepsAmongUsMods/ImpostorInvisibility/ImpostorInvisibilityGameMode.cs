using BepInEx.Configuration;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.CustomGameOptions;
using CheepsAmongUsMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ImpostorInvisibility
{
    public class ImpostorInvisibilityGameMode : GameMode
    {
        public const string Delimiter = "\n----------\n";

        private bool IsEnabled;
        private int LastEnabled;

        public const KeyCode UseKey = KeyCode.None;

        public int OnTime = 30;
        public int UseCooldown = 20;

        public List<byte> InvisiblePlayers = new List<byte>();

        private CustomButton EnableDisableInvisibility = null;
        private CustomText InvisibilityTextRenderer = null;

        private float ButtonOffsetX = -4.5f;
        private float ButtonOffsetY = -2.25f;
        private float TextOffsetX = -4.62f;
        private float TextOffsetY = -1.9f;

        public ImpostorInvisibilityGameMode(ConfigFile config) : base(14, "Impostor Invisibility") {
            #region ---------- Add Custom Options ----------
            new CustomNumberOption($"{Functions.ColorPurple}{GameModeName}", new string[] { "Options" });
            var onTime = new CustomNumberOption("Invisibility On Time", CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings);
            onTime.ValueChanged += OnTime_ValueChanged;
            onTime.Value = CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings.ToList().IndexOf(OnTime.ToString());

            var cooldown = new CustomNumberOption("Invisibility Cooldown", CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings);
            cooldown.ValueChanged += OnTime_ValueChanged;
            cooldown.Value = CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings.ToList().IndexOf(UseCooldown.ToString());
            #endregion

            #region ---------- Load Button and Text Position Offset ----------
            var loc = CheepsAmongUsMod.CheepsAmongUsMod.GetNewButtonTextLocation();

            ButtonOffsetX = loc.x;
            ButtonOffsetY = loc.y;
            TextOffsetX = loc.z;
            TextOffsetY = loc.w;
            #endregion

            RpcManager.RpcReceived += RpcManager_RpcReceived;
        }

        private void RpcManager_RpcReceived(object sender, RpcEventArgs e)
        {
            if (e.Command != ImpostorInvisibility.ImpostorInvisibilityRpc)
                return;

            var cmd = e.MessageReader.ReadByte();

            switch (cmd)
            {
                case 0:
                    {
                        OnTime = e.MessageReader.ReadInt32();
                        break;
                    }

                case 1:
                    {
                        UseCooldown = e.MessageReader.ReadInt32();
                        break;
                    }

                case 2:
                    {
                        var plId = e.MessageReader.ReadByte();
                        var enabled = Convert.ToBoolean(e.MessageReader.ReadByte());

                        if (enabled)
                            InvisiblePlayers.Add(plId);
                        else if (InvisiblePlayers.Contains(plId))
                        {
                            InvisiblePlayers.Remove(plId);

                            var player = PlayerController.FromPlayerId(plId);
                            player.SetOpacity(1);
                            player.IsVisible = true;
                        }
                        break;
                    }
            }
        }

        private void OnTime_ValueChanged(object sender, CustomNumberOption.CustomNumberOptionEventArgs e)
        {
            switch (e.NumberOption.TitleText)
            {
                case "Invisibility On Time":
                    {
                        int value = int.Parse(e.NumberOption.ValueStrings[e.NumberOption.Value]);

                        OnTime = value;
                        var writer = RpcManager.StartRpc(ImpostorInvisibility.ImpostorInvisibilityRpc);
                        writer.Write((byte)0);
                        writer.Write(OnTime);
                        writer.EndMessage();
                        break;
                    }
                case "Invisibility Cooldown":
                    {
                        int value = int.Parse(e.NumberOption.ValueStrings[e.NumberOption.Value]);

                        UseCooldown = value;
                        var writer = RpcManager.StartRpc(ImpostorInvisibility.ImpostorInvisibilityRpc);
                        writer.Write((byte)1);
                        writer.Write(UseCooldown);
                        writer.EndMessage();
                        break;
                    }
            }
        }

        public override void SyncSettings()
        {
            base.SyncSettings();

            var writerOnTime = RpcManager.StartRpc(ImpostorInvisibility.ImpostorInvisibilityRpc);
            writerOnTime.Write((byte)0);
            writerOnTime.Write(OnTime);
            writerOnTime.EndMessage();

            var writerCooldown = RpcManager.StartRpc(ImpostorInvisibility.ImpostorInvisibilityRpc);
            writerCooldown.Write((byte)1);
            writerCooldown.Write(UseCooldown);
            writerCooldown.EndMessage();
        }

        public override void OnStart()
        {
            base.OnStart();

            if (CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
            {
                var writerOnTime = RpcManager.StartRpc(ImpostorInvisibility.ImpostorInvisibilityRpc);
                writerOnTime.Write((byte)0);
                writerOnTime.Write(OnTime);
                writerOnTime.EndMessage();

                var writerCooldown = RpcManager.StartRpc(ImpostorInvisibility.ImpostorInvisibilityRpc);
                writerCooldown.Write((byte)1);
                writerCooldown.Write(UseCooldown);
                writerCooldown.EndMessage();
            }
        }

        public override void Loop()
        {
            base.Loop();

            GameModeSettingsAddition = 
                $"\nAbility Active Period: {Functions.ColorPurple}{OnTime}s[]\n" +
                $"Ability Cooldown: {Functions.ColorPurple}{UseCooldown}s[]";

            if (!IsInGame)
                return;

            foreach (var playerId in InvisiblePlayers)
            {
                PlayerController player = PlayerController.FromPlayerId(playerId);

                if (PlayerController.LocalPlayer.PlayerData.IsImpostor || PlayerController.LocalPlayer.PlayerData.IsDead)
                    player.SetOpacity(0.5f);
                else
                    player.IsVisible = false;
            }

            if (!PlayerController.LocalPlayer.PlayerData.IsImpostor)
                return;

            if (EnableDisableInvisibility == null)
                EnableDisableInvisibility = new CustomButton(ButtonOffsetX, ButtonOffsetY, Functions.LoadSpriteFromAssemblyResource(Assembly.GetExecutingAssembly(), "ImpostorInvisibility.Assets.invisibility.png"));

            if (InvisibilityTextRenderer == null)
            {
                InvisibilityTextRenderer = new CustomText(TextOffsetX, TextOffsetY, $"{UseCooldown}");
                InvisibilityTextRenderer.TextRenderer.scale = 2;
                InvisibilityTextRenderer.TextRenderer.Centered = true;
                InvisibilityTextRenderer.SetActive(false);
            }

            if (PlayerController.LocalPlayer.PlayerData.IsDead)
            {
                if (InvisibilityTextRenderer.Active)
                    InvisibilityTextRenderer.SetActive(false);

                if (EnableDisableInvisibility.Active)
                    EnableDisableInvisibility.SetActive(false);

                return;
            }

            string toDisplay = "--- [7a31f7ff]Impostor Invisibility[]---\n";

            if (IsEnabled)
            {
                toDisplay += $"Visible in [11c5edff]{ OnTime - (Functions.GetUnixTime() - LastEnabled) }s[]";

                InvisibilityTextRenderer.SetActive(true);
                InvisibilityTextRenderer.Text = $"{ OnTime - (Functions.GetUnixTime() - LastEnabled) }";
                InvisibilityTextRenderer.Color = new Color(0, 1, 0);

                if (OnTime - (Functions.GetUnixTime() - LastEnabled) <= 0)
                {
                    PlayerController ctrl = PlayerController.LocalPlayer;
                    IsEnabled = false;

                    RpcManager.SendRpc(ImpostorInvisibility.ImpostorInvisibilityRpc, new byte[] { 2, ctrl.PlayerId, Convert.ToByte(IsEnabled) });

                    if (IsEnabled)
                        InvisiblePlayers.Add(ctrl.PlayerId);
                    else if (InvisiblePlayers.Contains(ctrl.PlayerId))
                    {
                        InvisiblePlayers.Remove(ctrl.PlayerId);

                        var player = PlayerController.FromPlayerId(ctrl.PlayerId);
                        player.SetOpacity(1);

                        if(!player.PlayerControl.inVent)
                            player.IsVisible = true;
                    }
                }
            }
            else
            {
                bool CanEnable = LastEnabled + OnTime + UseCooldown <= Functions.GetUnixTime();

                if (CanEnable)
                {
                    // ----- Can enable demat -----
                    if (((EnableDisableInvisibility.InBounds && Functions.GetKey(KeyCode.Mouse0)) || Functions.GetKey(UseKey)) && PlayerController.LocalPlayer.Moveable)
                    {
                        // ----- User pressed key -----
                        LastEnabled = Functions.GetUnixTime();

                        PlayerController ctrl = PlayerController.LocalPlayer;
                        IsEnabled = true;

                        EnableDisableInvisibility.SpriteRenderer.color = new Color(0.75f, 0.75f, 0.75f, 0.75f);

                        RpcManager.SendRpc(ImpostorInvisibility.ImpostorInvisibilityRpc, new byte[] { 2, ctrl.PlayerId, Convert.ToByte(IsEnabled) });

                        if (IsEnabled)
                            InvisiblePlayers.Add(ctrl.PlayerId);
                        else if (InvisiblePlayers.Contains(ctrl.PlayerId))
                        {
                            InvisiblePlayers.Remove(ctrl.PlayerId);

                            var player = PlayerController.FromPlayerId(ctrl.PlayerId);
                            player.SetOpacity(1);

                            if (!player.PlayerControl.inVent)
                                player.IsVisible = true;
                        }
                    }
                    else
                    {
                        // ----- Display can press key -----
                        toDisplay += $"Use [11c5edff]{ UseKey }[] to activate.";
                        EnableDisableInvisibility.SpriteRenderer.color = new Color(1, 1, 1);
                        InvisibilityTextRenderer.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // ----- Cannot enable demat -----
                    toDisplay += $"Cooldown: [11c5edff]{ (UseCooldown + OnTime) - (Functions.GetUnixTime() - LastEnabled) }s[]";

                    InvisibilityTextRenderer.Text = $"{ (UseCooldown + OnTime) - (Functions.GetUnixTime() - LastEnabled) }";
                    InvisibilityTextRenderer.Color = new Color(1, 1, 1);
                }
            }

            string curText = PlayerHudManager.TaskText;

            if (!curText.Contains(Delimiter))
            {
                PlayerHudManager.TaskText = toDisplay + Delimiter + PlayerHudManager.TaskText;
            }
            else if (!curText.Contains(toDisplay))
            {
                string toReplace = curText.Split(new string[] { Delimiter }, StringSplitOptions.None)[0];

                PlayerHudManager.TaskText = curText.Replace(toReplace, toDisplay);
            }
        }

        public override void OnEnd()
        {
            base.OnEnd();

            foreach (var playerId in InvisiblePlayers)
            {
                var player = PlayerController.FromPlayerId(playerId);
                player.SetOpacity(1);
                player.IsVisible = true;
            }

            InvisiblePlayers.Clear();

            if (EnableDisableInvisibility != null)
            {
                EnableDisableInvisibility.Destroy();
                EnableDisableInvisibility = null;

                InvisibilityTextRenderer.Destroy();
                InvisibilityTextRenderer = null;
            }
        }
    }
}
