using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.Events;
using CheepsAmongUsApi.API.RegionApi;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
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
        public const string PluginVersion = "1.1.72";

        public const string ServerName = "Cheep-YT.com";
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

            #region ---------- Add custom regions ----------
            new Region(ServerName, ServerIP, new Server(ServerName, ServerIP, ServerPort)).InsertRegion(0); // Add Cheep-YT.com

            string serversPath = $"BepInEx/{PluginName}/servers/"; // Path to servers

            Directory.CreateDirectory(serversPath); // Create directory, if it does not exist

            if(!File.Exists(serversPath + "README.txt")) // Create a readme, if it does not exist
            {
                File.AppendAllText(serversPath + "README.txt",
                    "This directory can be used to add additional servers.\r\n" +
                    "\r\n" +
                    "To add a server, create a .txt file and name it, how you want the server to be called.\r\n" +
                    "Within this file simply enter the ip adress and port like this:\r\n" +
                    "207.180.234.175:22023");
            }

            foreach(var file in Directory.GetFiles(serversPath).Where(x => !Path.GetFileName(x).Equals("README.txt") && x.EndsWith(".txt"))) // Get all files in directory
            {
                var server = File.ReadAllLines(file)[0].Split(':'); // Get the first line of the file

                try
                {
                    string ip = server[0];  // Get the servers ip
                    ushort port = ushort.Parse(server[1]);  // Get the servers port

                    string name = Path.GetFileNameWithoutExtension(file); // Get the server name

                    new Region(name, ip, new Server(name, ip, port)).AddRegion(); // Add region
                }
                catch(Exception e) {
                    _logger.LogError("Failed to parse server " + file + ": " + e);
                }
            }
            #endregion

            #region ---------- Start Command Manager ----------
            ClientCommands.ClientCommandManager.Start();
            #endregion

            #region ---------- Start Role Manager ----------
            RoleManager.Start();
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

            #region ---------- Clear Values GameEnd ----------
            GameStartedEvent.Listener += () =>
            {
                PlayerHudManager.SetVictoryText(string.Empty);
                PlayerHudManager.SetDefeatText(string.Empty);
                GameSettingsAddition = string.Empty;
            };
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

            #region ---------- Transmit GameMode on Sync Settings ----------
            SyncedSettingsEvent.Listener += () =>
            {
                Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    SendModCommand("GameMode", ActiveGameMode);
                });
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
            PlayerController.LocalPlayer.RpcSendChat($"MessageFrom{PluginName}:{cmd}={val}"); 
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
        /// Returns the player with the lowest value player id
        /// </summary>
        public static PlayerController DecidingClient
        {
            get
            {
                PlayerController decidingPlayer = PlayerController.AllPlayerControls[0];

                foreach (var player in PlayerController.AllPlayerControls)
                    if (player.PlayerId < decidingPlayer.PlayerId)
                        decidingPlayer = player;

                return decidingPlayer;
            }
        }

        /// <summary>
        /// Returns true, if the local player is the player with the lowest value player id
        /// </summary>
        public static bool IsDecidingClient
        {
            get
            {
                return DecidingClient.IsLocalPlayer;
            }
        }

        /// <summary>
        /// Returns the player with the lowest value net id
        /// </summary>
        /// <returns></returns>
        [Obsolete("Replaced with DecidingClient")]
        public static PlayerController GetDecidingPlayer()
        {
            PlayerController decidingPlayer = PlayerController.AllPlayerControls[0];

            foreach (var player in PlayerController.AllPlayerControls)
                if (player.NetId < decidingPlayer.NetId)
                    decidingPlayer = player;

            return decidingPlayer;
        }

        /// <summary>
        /// Returns true, if the local player is the player with the lowest value net id
        /// </summary>
        /// <returns></returns>
        [Obsolete("Replaced with IsDecidingClient")]
        public static bool AmDecidingPlayer()
        {
            return GetDecidingPlayer().IsLocalPlayer;
        }
    }
}
