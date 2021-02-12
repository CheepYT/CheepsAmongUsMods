using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.CustomGameOptions;
using CheepsAmongUsApi.API.Enumerations;
using CheepsAmongUsApi.API.Events;
using CheepsAmongUsApi.API.RegionApi;
using CheepsAmongUsMod.API;
using CheepsAmongUsMod.Commands;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GameObjectClass = KHNHJFFECBP;

namespace CheepsAmongUsMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency(CheepsAmongUsApi.CheepsAmongUs.PluginGuid)]
    [BepInProcess("Among Us.exe")]
    public class CheepsAmongUsMod : BasePlugin
    {
        public const string PluginGuid = "com.cheep_yt.amongusmod";
        public const string PluginName = "CheepsAmongUsMod";
        public const string PluginVersion = "2.0.1";

        public const string ServerName = "Cheep-YT.com";
        public const string ServerIP = "207.180.234.175";
        public const ushort ServerPort = 22023;

        public const bool CreatorsRelease = false;

        public static string[] TimeStrings = { "5", "10", "20", "30", "60", "70", "80", "90", "120", "180", "240", "300" };

        public static Dictionary<MapType, Vector2[]> RandomMapLocations = new Dictionary<MapType, Vector2[]>();

        public static int CurrentButtonIndex = 0;

        public static Vector4[] ButtonTextLocations =
        {
            // new Vector4(Button_X, Button_Y, Text_X, Text_Y),
                new Vector4(-4.5f, -2.25f, -4.62f, -1.9f),
                new Vector4(-3.3f, -2.25f, -3.42f, -1.9f),
                new Vector4(-2.1f, -2.25f, -2.22f, -1.9f),

                new Vector4(-4.5f, -1.05f, -4.62f, -0.7f),
                new Vector4(-3.3f, -1.05f, -3.42f, -0.7f),
                new Vector4(-2.1f, -1.05f, -2.22f, -0.7f),

                new Vector4(-4.5f, 0.25f, -4.62f, 0.5f),
                new Vector4(-3.3f, 0.25f, -3.42f, 0.5f),
                new Vector4(-2.1f, 0.25f, -2.22f, 0.5f),
        };

        public static Vector4 GetNewButtonTextLocation()
        {
            var btn = ButtonTextLocations[CurrentButtonIndex];

            CurrentButtonIndex++;

            if (CurrentButtonIndex >= ButtonTextLocations.Length)
                CurrentButtonIndex = 0;

            return btn;
        }

        public static ManualLogSource _logger = null;
        public static GameObjectClass CurrentGame = null;

        public static GameMode SelectedGameMode;

        public static CustomNumberOption HeaderNumberOption;
        public static CustomNumberOption GameModeNumberOption;

        public static string SettingsAddition = "";

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"{PluginName} v{PluginVersion} created by Cheep loaded");

            RandomMapLocations.Add(MapType.Polus,
                new Vector2[]
                {
                    new Vector2(3, -12),
                    new Vector2(9, -12),
                    new Vector2(2, -17),
                    new Vector2(2, -24),
                    new Vector2(12, -23),
                    new Vector2(12, -16),
                    new Vector2(24, -22.5f),
                    new Vector2(20, -17.5f),
                    new Vector2(20, -11),
                    new Vector2(35, -8),
                    new Vector2(40, -10),
                    new Vector2(37, -21),
                    new Vector2(17, -3)
                });

            RandomMapLocations.Add(MapType.Skeld,
                new Vector2[]
                {
                    new Vector2(-1, -1),
                    new Vector2(-8.8f, -3),
                    new Vector2(-13.5f, -4.5f),
                    new Vector2(-20.5f, -6),
                    new Vector2(-17.5f, 2.5f),
                    new Vector2(-2, -15.5f),
                    new Vector2(9.4f, 2.8f),
                    new Vector2(9.4f, -12),
                    new Vector2(4, -15.5f),
                    new Vector2(-7.5f, -8.5f),
                    new Vector2(-15, -12),
                    new Vector2(6.5f, -3.5f),
                    new Vector2(16.5f, -5)
                });

            RandomMapLocations.Add(MapType.MiraHQ,
                new Vector2[] {
                    new Vector2(-4, 2.5f),
                    new Vector2(15, 0),
                    new Vector2(6, 1.5f),
                    new Vector2(6, 6),
                    new Vector2(2.4f, 11.5f),
                    new Vector2(8.5f, 13),
                    new Vector2(15, 4),
                    new Vector2(20, 4.5f),
                    new Vector2(23.8f, -1.7f),
                    new Vector2(25.5f, 3),
                    new Vector2(17.7f, 11.5f),
                    new Vector2(21, 20.6f),
                    new Vector2(14.8f, 20.3f),
                    new Vector2(17.8f, 23.8f)
                });

            _logger.LogInfo("Enabling harmony");
            #region ---------- Enable Harmony Patching ----------
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            #endregion

            _logger.LogInfo("Loading regions");
            #region ---------- Add custom regions ----------
            new Region(ServerName, ServerIP, new Server(ServerName, ServerIP, ServerPort)).InsertRegion(0); // Add Cheep-YT.com

            string serversPath = $"BepInEx/plugins/{PluginName}/servers/"; // Path to servers

            Directory.CreateDirectory(serversPath); // Create directory, if it does not exist

            if (!File.Exists(serversPath + "README.txt")) // Create a readme, if it does not exist
            {
                File.AppendAllText(serversPath + "README.txt",
                    "This directory can be used to add additional servers.\r\n" +
                    "\r\n" +
                    "To add a server, create a .txt file and name it, how you want the server to be called.\r\n" +
                    "Within this file simply enter the ip adress and port like this:\r\n" +
                    "207.180.234.175:22023");
            }

            foreach (var file in Directory.GetFiles(serversPath).Where(x => !Path.GetFileName(x).Equals("README.txt") && x.EndsWith(".txt"))) // Get all files in directory
            {
                var server = File.ReadAllLines(file)[0].Split(':'); // Get the first line of the file

                try
                {
                    string ip = server[0];  // Get the servers ip
                    ushort port = ushort.Parse(server[1]);  // Get the servers port

                    string name = Path.GetFileNameWithoutExtension(file); // Get the server name

                    new Region(name, ip, new Server(name, ip, port)).AddRegion(); // Add region
                }
                catch (Exception e)
                {
                    _logger.LogError("Failed to parse server " + file + ": " + e);
                }
            }
            #endregion

            _logger.LogInfo("Setting up custom options");
            #region ----------- Setup Custom Options -----------

            HeaderNumberOption = new CustomNumberOption(Functions.ColorCyan + PluginName, new string[] { $"{GameMode.AvailableGameModes.Count - 1} Modes" });

            GameModeNumberOption = new CustomNumberOption("GameMode", GameMode.ShortGameModeNames);
            GameModeNumberOption.ValueChanged += GameModeManager.GameModeNumberOption_ValueChanged;

            var mapObject = new CustomNumberOption("Map", Enum.GetNames(typeof(MapType)));
            mapObject.ValueChanged += CustomNumberOption_ValueChanged; ;
            mapObject.Value = (int)GameOptions.Map;

            var impostorsObject = new CustomNumberOption("Impostor Count", new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" });
            impostorsObject.Value = 1;
            impostorsObject.ValueChanged += CustomNumberOption_ValueChanged;

            var maxPlayerObj = new CustomNumberOption("Max Players", new string[] { "4", "5", "10", "15", "20", "30", "50", "70", "100", "125" });
            impostorsObject.Value = 2;
            maxPlayerObj.ValueChanged += CustomNumberOption_ValueChanged;

            #endregion

            #region ---------- Update CustomNumberOptions ----------
            LobbyBehaviourStartedEvent.Listener += () =>
            {
                CurrentButtonIndex = 0;

                Task.Run(async () =>
                {
                    await Task.Delay(2000);

                    mapObject.Value = (int)GameOptions.Map;

                    impostorsObject.Value = impostorsObject.ValueStrings.IndexOf(GameOptions.ImpostorCount.ToString());

                    maxPlayerObj.Value = maxPlayerObj.ValueStrings.IndexOf(GameOptions.MaxPlayers.ToString());
                });
            };
            #endregion

            _logger.LogInfo("Registering commands");
            #region ---------- Register Commands ----------
            new HelpCommand();
            new GameOptionsCommand();
            #endregion

            SelectedGameMode = new GameMode(0, "Default");

            GameModeManager.Start();    // Start GameModeManager
            RoleManager.Start();        // Start RoleManager
            RoleGameModeManager.Start();    // Start RoleGameModeManager
            Notifier.StartNotifier();   // Start Notifier

            if (Config.Bind("Debug", "enable_debug", true, "Enables the debug menu").Value)
                DebugMenu.Start();
        }

        #region --------------------------------- Custom Settings ---------------------------------
        private void CustomNumberOption_ValueChanged(object sender, CustomNumberOption.CustomNumberOptionEventArgs e)
        {
            switch (e.NumberOption.TitleText)
            {
                case "Map":
                    {
                        GameOptions.Map = (MapType)e.NumberOption.Value;
                        GameOptions.RpcSyncSettings();

                        break;
                    }

                case "Impostor Count":
                    {
                        GameOptions.ImpostorCount = int.Parse(e.NumberOption.Selected);
                        GameOptions.RpcSyncSettings();

                        break;
                    }

                case "Max Players":
                    {
                        GameOptions.MaxPlayers = int.Parse(e.NumberOption.Selected);
                        GameOptions.RpcSyncSettings();

                        break;
                    }
            }
        }
        #endregion

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
    }

    public class ModPatching
    {
        #region -------------------- Game Connect Event --------------------
        [HarmonyPatch(typeof(GameObjectClass), nameof(GameObjectClass.Connect))]
        public static class Patch_GameObjectClass
        {
            public static void Postfix(GameObjectClass __instance)
            {
                CheepsAmongUsMod.CurrentGame = __instance;
            }
        }
        #endregion
    }
}
