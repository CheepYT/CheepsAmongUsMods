using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using CheepsAmongUsApi.API.RegionApi;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using GameObjectClass = KHNHJFFECBP;

namespace CheepsAmongUsBaseMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]
    [BepInProcess("Among Us.exe")]
    public class CheepsAmongUsBaseMod : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusbasemod";
        public const string PluginName = "CheepsAmongUsMod";
        public const string PluginVersion = "1.0.5";

        public const string ServerName = "Cheep-YT - Public";
        public const string ServerIP = "207.180.234.175";
        public const ushort ServerPort = 22023;

        public static ManualLogSource _logger = null;

        public static string ActiveGameMode = "Default";
        public static GameObjectClass CurrentGame = null;
        public static List<string> InstalledGameModes = new List<string>();

        public static string GameSettingsAddition = string.Empty;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            #region ---------- Enable Harmony Patching ----------
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            #region ---------- Add custom region ----------
            new Region(ServerName, ServerIP, new Server(ServerName, ServerIP, ServerPort)).InsertRegion(0);
            #endregion

            #region ---------- Start Command Manager ----------
            ClientCommands.ClientCommandManager.Start();
            #endregion

            #region ---------- Append Version Text ----------
            PlayerHudManager.AppendedVersionText = $" + [7a31f7ff]{PluginName} [3170f7ff]v{PluginVersion} []by [2200ffff]CheepYT";

            PlayerHudManager.AppendedPingText = $"\nMode: {Functions.ColorPurple}{ AddSpaces(ActiveGameMode) }[]\n" +
                $">> {Functions.ColorGreen}Cheep-YT.com[] <<";
            PlayerHudManager.IsPingTextCentered = true;

            #region ---------- Wait 1.5 seconds, to append gamemode count to version shower ----------
            Task.Run(async () =>
            {
                await Task.Delay(1500);
                PlayerHudManager.AppendedVersionText += $"\n[]Installed GameModes: {Functions.ColorOrange}{InstalledGameModes.Count - 1}";
            });
            #endregion

            InstalledGameModes.Add(ActiveGameMode);
            #endregion

            #region ---------- Lobby HUD ---------- 
            HudUpdateEvent.Listener += () =>
            {
                if (PlayerHudManager.HudManager == null)
                    return;

                string topText = $"{Functions.ColorPurple}{PluginName} {Functions.ColorBlue}v{PluginVersion} []by {Functions.ColorBlue}CheepYT[]\n" +
                $"GameMode: {Functions.ColorPurple}{ AddSpaces(ActiveGameMode) }[]" +
                GameSettingsAddition;

                string Delimiter = "\n\n";

                string curText = PlayerHudManager.GameSettingsText;

                if (!curText.Contains(Delimiter))
                {
                    PlayerHudManager.GameSettingsText = topText + Delimiter + PlayerHudManager.GameSettingsText;
                }
                else if (!curText.Contains(topText))
                {
                    string toReplace = curText.Split(new string[] { Delimiter }, StringSplitOptions.None)[0];

                    PlayerHudManager.GameSettingsText = curText.Replace(toReplace, topText);
                }
            };
            #endregion
        }

        /// <summary>
        /// Uses the chat to send and receive mod commands
        /// </summary>
        /// <param name="cmd">Command to send (must not contain ':' or '=')</param>
        /// <param name="val">Value to send (must not contain ':' or '=')</param>
        public static void SendModCommand(string cmd, string val)
        {
            PlayerController.GetLocalPlayer().RpcSendChat($"MessageFrom{PluginName}:{cmd}={val}"); 
        }

        /// <summary>
        /// Adds spaces in front of a capital letter
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string AddSpaces(string str)
        {
            char[] array = str.ToCharArray();

            int countInserts = 0;

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i].ToString().ToUpper() == array[i].ToString())
                {
                    str = str.Insert(i + countInserts, " ");
                    countInserts++;
                }
            }

            return str;
        }

        /// <summary>
        /// Returns the player with the lowest value net id
        /// </summary>
        /// <returns></returns>
        public static PlayerController GetDecidingPlayer()
        {
            PlayerController decidingPlayer = PlayerController.GetAllPlayers()[0];

            foreach (var player in PlayerController.GetAllPlayers())
                if (player.NetId < decidingPlayer.NetId)
                    decidingPlayer = player;

            return decidingPlayer;
        }

        /// <summary>
        /// Returns true, if the local player is the player with the lowest value net id
        /// </summary>
        /// <returns></returns>
        public static bool AmDecidingPlayer()
        {
            return GetDecidingPlayer().AmPlayerController();
        }
    }
}
