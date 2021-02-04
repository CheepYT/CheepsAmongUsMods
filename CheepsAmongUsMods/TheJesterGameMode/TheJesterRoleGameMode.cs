using BepInEx.Configuration;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.CustomGameOptions;
using CheepsAmongUsMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using GameEndReason = AIMMJPEOPEC;

namespace TheJesterGameMode
{
    public class TheJesterRoleGameMode : RoleGameMode
    {
        public int NumJesters = 0;

        public bool JestersWon = false;

        public System.Random Random = new System.Random();

        public TheJesterRoleGameMode(ConfigFile Config) : base()
        {
            #region ---------- Add Custom Options ----------
            new CustomNumberOption($"{Functions.ColorPurple}{TheJester.GameModeName}", new string[] { "Options" });

            List<string> JesterCount = new List<string>();

            for (int i = 0; i <= Config.Bind("Jester", "max_jesters", 2, "Change this, if you want to have more than 2 Jesters.").Value; i++)
                JesterCount.Add(i.ToString());

            new CustomNumberOption("Jester Count", JesterCount).ValueChanged += TheJesterRoleGameMode_ValueChanged; ;
            #endregion

            RpcManager.RpcReceived += RpcManager_RpcReceived;
        }

        private void RpcManager_RpcReceived(object sender, RpcEventArgs e)
        {
            if (e.Command != TheJester.JesterRpc)
                return;

            var cmd = e.MessageReader.ReadByte();

            switch (cmd)
            {
                case (byte)TheJester.CustomRpc.UpdateJesterCount:
                    {
                        NumJesters = e.MessageReader.ReadByte();
                        break;
                    }

                case (byte)TheJester.CustomRpc.SetJester:
                    {
                        setJester(PlayerController.FromPlayerId(e.MessageReader.ReadByte()));
                        break;
                    }

                case (byte)TheJester.CustomRpc.JesterExiled:
                    {
                        JesterWon();
                        break;
                    }
            }
        }

        internal void JesterWon()
        {
            JestersWon = true;

            var impostorCount = PlayerController.AllPlayerControls.Where(x => x.PlayerData.IsImpostor).Count();
            var crewCount = PlayerController.AllPlayerControls.Where(x => !x.PlayerData.IsImpostor).Count();

            if (impostorCount < crewCount && CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                Functions.RpcEndGame(GameEndReason.ImpostorByKill);
        }

        private void setJester(PlayerController jester)
        {
            RolePlayer JesterRolePlayer = new RolePlayer(jester, "Jester");
            JesterRolePlayer.RoleEjected.UseRoleEjected = true;

            jester.ClearTasks();
            jester.PlayerTaskObjects = new Il2CppSystem.Collections.Generic.List<PILBGHDHJLH>();

            var intro = JesterRolePlayer.RoleIntro;
            intro.UseRoleIntro = true;
            intro.RoleNameColor = new Color(0.74901960784f, 0, 1f);
            intro.BackgroundColor = new Color(127 / 255f, 0 / 255f, 186 / 255f);
            intro.RoleDescription =
                $"Trick the crewmates into thinking\n" +
                $"that you are an {Functions.ColorRed}Impostor[]\n";

            var outro = JesterRolePlayer.RoleOutro;
            outro.WinText = "Victory";
            outro.WinTextColor = new Color(175 / 255f, 43 / 255f, 237 / 255f);
            outro.BackgroundColor = new Color(127 / 255f, 0 / 255f, 186 / 255f);

            AllRolePlayers.Add(JesterRolePlayer);

            if(AllRolePlayers.Where(x => x.AmRolePlayer).Count() > 0)
                foreach(var role in AllRolePlayers)
                    role.PlayerController.PlayerControl.nameText.Color = new Color(0.74901960784f, 0, 1f);
        }

        private void TheJesterRoleGameMode_ValueChanged(object sender, CustomNumberOption.CustomNumberOptionEventArgs e)
        {
            NumJesters = int.Parse(e.NumberOption.Selected);
            sync();
        }

        private void sync()
        {
            RpcManager.SendRpc(TheJester.JesterRpc, new byte[] { (byte)TheJester.CustomRpc.UpdateJesterCount, (byte)NumJesters });
        }

        public override void SyncSettings()
        {
            base.SyncSettings();
            sync();
        }

        public override void OnStart()
        {
            base.OnStart();

            if (CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                sync();
        }

        string prevJesters = "REPLACEME";

        public override void Loop()
        {
            base.Loop();

            string jesters = $"\nJesters: {Functions.ColorPurple}{NumJesters}[]";

            if (!CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition.Contains(jesters) && NumJesters > 0)
                CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition = CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition.Replace(prevJesters, "") + jesters;
            else if (CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition.Contains(prevJesters) && NumJesters == 0)
                CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition = CheepsAmongUsMod.CheepsAmongUsMod.SettingsAddition.Replace(prevJesters, "");

            prevJesters = jesters;
        }

        public override void OnSetInfected()
        {
            base.OnSetInfected();

            if (NumJesters == 0 || !CheepsAmongUsMod.CheepsAmongUsMod.IsDecidingClient)
                return;

            var available = PlayerController.AllPlayerControls.Where(x => !x.PlayerData.IsImpostor && !RoleManager.HasPlayerAnyRole(x)).ToList();

            for(int i = 0; i < NumJesters; i++)
                if(available.Count > 0)
                {
                    var player = available[Random.Next(available.Count)];

                    if (RoleManager.HasPlayerAnyRole(player))
                    {
                        i--;
                        continue;
                    }

                    available.Remove(player);

                    RpcManager.SendRpc(TheJester.JesterRpc, new byte[] { (byte)TheJester.CustomRpc.SetJester, player.PlayerId });
                    setJester(player);
                }
        }

        public override void ResetValues()
        {
            base.ResetValues();
            JestersWon = false;
        }
    }
}
