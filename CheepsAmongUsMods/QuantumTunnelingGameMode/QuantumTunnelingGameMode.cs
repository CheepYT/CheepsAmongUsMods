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

namespace QuantumTunnelingGameMode
{
    public class QuantumTunnelingGameMode : GameMode
    {
        public const string Delimiter = "\n----------\n";

        private bool IsEnabled;
        private int LastEnabled;

        public const KeyCode UseKey = KeyCode.None;

        public int OnTime = 30;
        public int UseCooldown = 20;

        private CustomButton EnableDisableCollision = null;
        private CustomText CollisionTextRenderer = null;

        private float ButtonOffsetX = -4.5f;
        private float ButtonOffsetY = -2.25f;
        private float TextOffsetX = -4.62f;
        private float TextOffsetY = -1.9f;

        public QuantumTunnelingGameMode(ConfigFile config) : base(15, "Quantum Tunneling")
        {
            #region ---------- Add Custom Options ----------
            new CustomNumberOption($"{Functions.ColorPurple}{GameModeName}", new string[] { "Options" });
            var onTime = new CustomNumberOption("Ability On Time", CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings);
            onTime.ValueChanged += OnTime_ValueChanged;
            onTime.Value = CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings.ToList().IndexOf(OnTime.ToString());

            var cooldown = new CustomNumberOption("Ability Cooldown", CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings);
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
            if (e.Command != QuantumTunneling.QuantumTunnelingRpc)
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

                        var player = PlayerController.FromPlayerId(plId);

                        player.HasCollision = !enabled;
                        break;
                    }
            }
        }

        private void OnTime_ValueChanged(object sender, CustomNumberOption.CustomNumberOptionEventArgs e)
        {
            switch (e.NumberOption.TitleText)
            {
                case "Ability On Time":
                    {
                        int value = int.Parse(e.NumberOption.ValueStrings[e.NumberOption.Value]);

                        OnTime = value;
                        var writer = RpcManager.StartRpc(QuantumTunneling.QuantumTunnelingRpc);
                        writer.Write((byte)0);
                        writer.Write(OnTime);
                        writer.EndMessage();
                        break;
                    }
                case "Ability Cooldown":
                    {
                        int value = int.Parse(e.NumberOption.ValueStrings[e.NumberOption.Value]);

                        UseCooldown = value;
                        var writer = RpcManager.StartRpc(QuantumTunneling.QuantumTunnelingRpc);
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

            var writerOnTime = RpcManager.StartRpc(QuantumTunneling.QuantumTunnelingRpc);
            writerOnTime.Write((byte)0);
            writerOnTime.Write(OnTime);
            writerOnTime.EndMessage();

            var writerCooldown = RpcManager.StartRpc(QuantumTunneling.QuantumTunnelingRpc);
            writerCooldown.Write((byte)1);
            writerCooldown.Write(UseCooldown);
            writerCooldown.EndMessage();
        }

        public override void OnStart()
        {
            base.OnStart();

            if (CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
            {
                var writerOnTime = RpcManager.StartRpc(QuantumTunneling.QuantumTunnelingRpc);
                writerOnTime.Write((byte)0);
                writerOnTime.Write(OnTime);
                writerOnTime.EndMessage();

                var writerCooldown = RpcManager.StartRpc(QuantumTunneling.QuantumTunnelingRpc);
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

            if (!PlayerController.LocalPlayer.PlayerData.IsImpostor)
                return;

            if (EnableDisableCollision == null)
                EnableDisableCollision = new CustomButton(ButtonOffsetX, ButtonOffsetY, Functions.LoadSpriteFromAssemblyResource(Assembly.GetExecutingAssembly(), "QuantumTunnelingGameMode.Assets.collision.png"));

            if (CollisionTextRenderer == null)
            {
                CollisionTextRenderer = new CustomText(TextOffsetX, TextOffsetY, $"{UseCooldown}");
                CollisionTextRenderer.TextRenderer.scale = 2;
                CollisionTextRenderer.TextRenderer.Centered = true;
                CollisionTextRenderer.SetActive(false);
            }

            if (PlayerController.LocalPlayer.PlayerData.IsDead)
            {
                if (CollisionTextRenderer.Active)
                    CollisionTextRenderer.SetActive(false);

                if (EnableDisableCollision.Active)
                    EnableDisableCollision.SetActive(false);

                return;
            }

            string toDisplay = "--- [7a31f7ff]Quantum Tunneling[]---\n";

            if (IsEnabled)
            {
                toDisplay += $"Collision in [11c5edff]{ OnTime - (Functions.GetUnixTime() - LastEnabled) }s[]";

                CollisionTextRenderer.SetActive(true);
                CollisionTextRenderer.Text = $"{ OnTime - (Functions.GetUnixTime() - LastEnabled) }";
                CollisionTextRenderer.Color = new Color(0, 1, 0);

                if (OnTime - (Functions.GetUnixTime() - LastEnabled) <= 0)
                {
                    PlayerController ctrl = PlayerController.LocalPlayer;
                    IsEnabled = false;

                    RpcManager.SendRpc(QuantumTunneling.QuantumTunnelingRpc, new byte[] { 2, ctrl.PlayerId, Convert.ToByte(IsEnabled) });
                    PlayerHudManager.HudManager.ShadowQuad.gameObject.SetActive(!IsEnabled);

                    ctrl.HasCollision = !IsEnabled;
                }
            }
            else
            {
                bool CanEnable = LastEnabled + OnTime + UseCooldown <= Functions.GetUnixTime();

                if (CanEnable)
                {
                    // ----- Can enable demat -----
                    if (((EnableDisableCollision.InBounds && Functions.GetKey(KeyCode.Mouse0)) || Functions.GetKey(UseKey)) && PlayerController.LocalPlayer.Moveable)
                    {
                        // ----- User pressed key -----
                        LastEnabled = Functions.GetUnixTime();

                        PlayerController ctrl = PlayerController.LocalPlayer;
                        IsEnabled = true;

                        EnableDisableCollision.SpriteRenderer.color = new Color(0.75f, 0.75f, 0.75f, 0.75f);

                        RpcManager.SendRpc(QuantumTunneling.QuantumTunnelingRpc, new byte[] { 2, ctrl.PlayerId, Convert.ToByte(IsEnabled) });
                        PlayerHudManager.HudManager.ShadowQuad.gameObject.SetActive(!IsEnabled);

                        ctrl.HasCollision = !IsEnabled;
                    }
                    else
                    {
                        // ----- Display can press key -----
                        toDisplay += $"Use [11c5edff]{ UseKey }[] to activate.";
                        EnableDisableCollision.SpriteRenderer.color = new Color(1, 1, 1);
                        CollisionTextRenderer.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // ----- Cannot enable demat -----
                    toDisplay += $"Cooldown: [11c5edff]{ (UseCooldown + OnTime) - (Functions.GetUnixTime() - LastEnabled) }s[]";

                    CollisionTextRenderer.Text = $"{ (UseCooldown + OnTime) - (Functions.GetUnixTime() - LastEnabled) }";
                    CollisionTextRenderer.Color = new Color(1, 1, 1);
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

            if (EnableDisableCollision != null)
            {
                EnableDisableCollision.Destroy();
                EnableDisableCollision = null;

                CollisionTextRenderer.Destroy();
                CollisionTextRenderer = null;
            }
        }

        public override void ResetValues()
        {
            base.ResetValues();
            CollisionTextRenderer = null;
            EnableDisableCollision = null;
        }
    }
}
