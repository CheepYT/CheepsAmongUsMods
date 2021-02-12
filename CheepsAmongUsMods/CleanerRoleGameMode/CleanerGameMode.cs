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


using DeadBody = DDPGLPLGFOI;

namespace CleanerRoleGameMode
{
    public class CleanerGameMode : RoleGameMode
    {
        public enum CustomRpc : byte
        {
            UpdateSettings = 0,
            SetCleaner = 1,
            CleanBody = 2,
            UpdateCooldown = 3
        };

        public bool Enabled { get; private set; }

        public const float CleanDistance = 1.2f;

        public Sprite CleanButtonSprite = null;

        public Sprite KillSprite = null;

        public Color DisabledColor = new Color(1f, 1f, 1f, 0.3f);

        public Color EnabledColor = new Color(1f, 1f, 1f, 1f);

        public int CleanCooldown = 30;

        public CustomStopwatch CleanStopwatch = null;

        public CleanerGameMode() : base()
        {
            #region ---------- Add Custom Options ----------
            new CustomNumberOption($"{Functions.ColorPurple}{Cleaner.GameModeName}", new string[] { "Options" });
            new CustomToggleOption($"Activate Cleaner Role").ValueChanged += CleanerGameMode_ValueChanged;
            
            var cooldown = new CustomNumberOption($"Clean Cooldown", CheepsAmongUsMod.CheepsAmongUsMod.TimeStrings);

            cooldown.Value = cooldown.ValueStrings.IndexOf(CleanCooldown.ToString());
            cooldown.ValueChanged += Cooldown_ValueChanged;

            #endregion

            RpcManager.RpcReceived += RpcManager_RpcReceived; // Add Rpc Received Listener
        }

        private void Cooldown_ValueChanged(object sender, CustomNumberOption.CustomNumberOptionEventArgs e)
        {
            int value = int.Parse(e.NumberOption.Selected);

            CleanCooldown = value;

            sync();
        }

        private void RpcManager_RpcReceived(object sender, RpcEventArgs e)
        {
            if (e.Command != Cleaner.CleanerRpc)
                return;

            var cmd = e.MessageReader.ReadByte();

            switch (cmd)
            {
                case (byte)CustomRpc.UpdateSettings:
                    {
                        Enabled = Convert.ToBoolean(e.MessageReader.ReadByte());
                        break;
                    }

                case (byte)CustomRpc.SetCleaner:
                    {
                        var ctrl = PlayerController.FromPlayerId(e.MessageReader.ReadByte());

                        setCleaner(ctrl);
                        break;
                    }

                case (byte)CustomRpc.CleanBody:
                    {
                        var id = e.MessageReader.ReadByte();

                        removeBody(id);

                        break;
                    }

                case (byte)CustomRpc.UpdateCooldown:
                    {
                        CleanCooldown = e.MessageReader.ReadInt32();

                        break;
                    }
            }
        }

        private void CleanerGameMode_ValueChanged(object sender, CustomToggleOption.CustomToggleOptionEventArgs e)
        {
            Enabled = e.ToggleOption.Value;
            sync();
        }

        private void sync()
        {
            RpcManager.SendRpc(Cleaner.CleanerRpc, new byte[] { (byte)CustomRpc.UpdateSettings, Convert.ToByte(Enabled) });

            var writer = RpcManager.StartRpc(Cleaner.CleanerRpc);
            writer.Write((byte)CustomRpc.UpdateCooldown);
            writer.Write(CleanCooldown);
            writer.EndMessage();
        }

        private void setCleaner(PlayerController ctrl)
        {
            var role = new RolePlayer(ctrl, "Cleaner");

            role.RoleIntro.UseRoleIntro = true;
            role.RoleIntro.BackgroundColor = new Color(0, 136 / 255f, 255 / 255f);
            role.RoleIntro.RoleNameColor = new Color(0, 221 / 255f, 255 / 255f);
            role.RoleIntro.RoleDescription = "Help the [ff0000ff]Impostors[] by [0088ffff]cleaning []the [ff0000ff]crime scene[].";

            role.RoleEjected.UseRoleEjected = true;

            role.NameColor = new Color(0, 221 / 255f, 255 / 255f);

            role.NameColorVisible = PlayerController.LocalPlayer.PlayerData.IsImpostor;

            AllRolePlayers.Add(role);

            if (ctrl.IsLocalPlayer)
                CleanStopwatch = new CustomStopwatch(true);
        }

        public void removeBody(byte bodyId)
        {
            foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                if (body.ParentId == bodyId)
                {
                    body.gameObject.SetActive(false);
                    UnityEngine.Object.Destroy(body);
                }
        }

        public override void SyncSettings()
        {
            base.SyncSettings();

            sync();
        }

        public override void ResetValues()
        {
            base.ResetValues();

            CleanButtonSprite = null;

            if (KillSprite != null)
            {
                PlayerHudManager.HudManager.KillButton.renderer.sprite = KillSprite;
                KillSprite = null;
            }
        }

        public override void OnStart()
        {
            base.OnStart();

            if (CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                sync();
        }

        string prevAdd = "REPLACEME";

        public override void OnSetInfected()
        {
            base.OnSetInfected();

            if (PlayerController.AllPlayerControls.Where(x => x.PlayerData.IsImpostor).Count() < 2)
                return;

            var available = PlayerController.AllPlayerControls.Where(x => x.PlayerData.IsImpostor && !RoleManager.HasPlayerAnyRole(x)).ToList();
            var ctrl = available[RoleGameModeManager.Random.Next(available.Count)];

            setCleaner(ctrl);

            RpcManager.SendRpc(Cleaner.CleanerRpc, new byte[] { (byte)CustomRpc.SetCleaner, ctrl.PlayerId });
        }

        private void SetOutline(byte bodyId, Color color, bool enabled)
        {
            float active = enabled ? 1 : 0;

            SpriteRenderer renderer = null;

            foreach (var body in UnityEngine.Object.FindObjectsOfType<DeadBody>())
                if (body.ParentId == bodyId)
                    renderer = body.GetComponent<SpriteRenderer>();

            renderer.material.SetFloat("_Outline", active);

            if (enabled)
                renderer.material.SetColor("_OutlineColor", color);
        }

        public override void Loop()
        {
            base.Loop();

            string add = $"\nCleaner Role: {Functions.ColorPurple}Active[]\n[]Clean Cooldown: {Functions.ColorPurple}{CleanCooldown}s";

            if (!CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition.Contains(add) && Enabled)
                CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition = CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition.Replace(prevAdd, "") + add;
            else if (CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition.Contains(prevAdd) && !Enabled)
                CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition = CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition.Replace(prevAdd, "");

            prevAdd = add;

            if (AllRolePlayers.Count == 0)
                return;

            var cleaner = AllRolePlayers.First();

            if (PlayerController.LocalPlayer.PlayerData.IsImpostor)
                cleaner.PlayerController.PlayerControl.nameText.Color = cleaner.NameColor;

            if (!cleaner.AmRolePlayer)
                return;

            if (CleanButtonSprite == null)
            {
                CleanButtonSprite = Functions.LoadSpriteFromAssemblyResource(Assembly.GetExecutingAssembly(), "CleanerRoleGameMode.Assets.clean.png"); // Load sprite

                KillSprite = PlayerHudManager.HudManager.KillButton.renderer.sprite;

                PlayerHudManager.HudManager.KillButton.renderer.sprite = CleanButtonSprite;
            }

            if (CleanStopwatch.Elapsed.TotalSeconds < CleanCooldown)
                PlayerHudManager.HudManager.KillButton.SetCoolDown(CleanCooldown - (float)CleanStopwatch.Elapsed.TotalSeconds, CleanCooldown);
            else
                PlayerHudManager.HudManager.KillButton.SetCoolDown(0, CleanCooldown);

            #region ---------- Manage Dead Body Target ----------
            var bodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();

            DeadBody closest = null;

            #region ----- Find closest body -----
            foreach (var body in bodies)
            {
                try
                {
                    var distToObject = Vector2.Distance(body.transform.position, PlayerController.LocalPlayer.GameObject.transform.position);

                    var prevDist = float.MaxValue;

                    if(closest != null)
                        prevDist = Vector2.Distance(closest.transform.position, PlayerController.LocalPlayer.GameObject.transform.position);

                    if (distToObject < prevDist)
                    {
                        closest = body;
                    }
                }
                catch { }
            }
            #endregion

            #region ----- If distance too large, set target to null -----
            if(closest != null)
                if (Vector2.Distance(closest.transform.position, PlayerController.LocalPlayer.GameObject.transform.position) > CleanDistance)
                    closest = null;
            #endregion

            #region ----- Remove outline from previous target -----
            if (Patching.Patch_KillButtonManager_PerformKill.TargetId != byte.MaxValue)
            {
                if(closest == null || Patching.Patch_KillButtonManager_PerformKill.TargetId != closest.ParentId)
                    try
                    {
                        SetOutline(Patching.Patch_KillButtonManager_PerformKill.TargetId, Color.blue, false);
                    } catch { }
            }
            #endregion

            if (closest != null)
                Patching.Patch_KillButtonManager_PerformKill.TargetId = closest.ParentId; // update target
            else
                Patching.Patch_KillButtonManager_PerformKill.TargetId = byte.MaxValue;

            if (closest != null)
            {
                SetOutline(closest.ParentId, Color.blue, true); // set target outline
                PlayerHudManager.HudManager.KillButton.renderer.color = EnabledColor;
                PlayerHudManager.HudManager.KillButton.renderer.material.SetFloat("_Desat", 0f);
            } else
            {
                PlayerHudManager.HudManager.KillButton.renderer.color = DisabledColor;
                PlayerHudManager.HudManager.KillButton.renderer.material.SetFloat("_Desat", 1f);
            }
            #endregion

        }
    }
}
