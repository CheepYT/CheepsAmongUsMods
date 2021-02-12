using CheepsAmongUsApi.API;
using CheepsAmongUsApi.API.CustomGameOptions;
using CheepsAmongUsApi.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameStateEnum = KHNHJFFECBP.KGEKNMMAKKN;

namespace CheepsAmongUsMod.API
{
    public class GameMode
    {
        public const string AdvStr = ">> " + Functions.ColorGreen + "Cheep-YT.com[] <<";

        public static List<GameMode> AvailableGameModes = new List<GameMode>();

        public static List<string> ShortGameModeNames
        {
            get
            {
                List<string> GameModes = new List<string>();

                foreach (var gm in AvailableGameModes)
                {
                    var gamemode = gm.GameModeName;

                    if (gamemode.Length > 9)
                    {
                        gamemode = gamemode.Substring(0, 7) + "..";
                    }

                    GameModes.Add(gamemode);
                }

                return GameModes;
            }
        }

        public byte GameModeId { get; }

        public string GameModeName { get; }

        public string ModNotice { get; set; }

        public bool IsInGame
        {
            get
            {
                return CheepsAmongUsMod.CurrentGame != null && CheepsAmongUsMod.CurrentGame.GameState == GameStateEnum.Started && IsSelected && hasStarted;
            } 
        }

        public bool IsSelected
        {
            get
            {
                return GameModeManager.Selected.GameModeId == GameModeId;
            }
        }

        public string GameModeSettingsAddition { get; set; }

        private bool hasStarted = false;

        /// <summary>
        /// Creates a new instance of a gamemode
        /// </summary>
        /// <param name="id">The id of the gamemode</param>
        /// <param name="name">The display name of the gamemode</param>
        public GameMode(byte id, string name)
        {
            GameModeId = id;
            GameModeName = name;

            AvailableGameModes.Add(this);

            PlayerHudManager.AppendedVersionText =
                $"\n[7a31f7ff]{CheepsAmongUsMod.PluginName} [3170f7ff]v{CheepsAmongUsMod.PluginVersion} []by [2200ffff]CheepYT\n" +
                $"[]Installed GameModes: {Functions.ColorOrange}{AvailableGameModes.Count - 1}[]\n" +
                $"[]Installed Roles: {Functions.ColorOrange}{RoleGameMode.AllGameModes.Count}[]";

            CheepsAmongUsMod.GameModeNumberOption.ValueStrings = ShortGameModeNames;
            CheepsAmongUsMod.HeaderNumberOption.ValueStrings[0] = $"{AvailableGameModes.Count - 1} Modes";
    }

        /// <summary>
        /// Executed, once the game starts
        /// </summary>
        public virtual void OnStart() {
            hasStarted = true;
        }

        /// <summary>
        /// Executed by the host, once infected players are set
        /// </summary>
        public virtual void OnSetInfected() { }

        /// <summary>
        /// This function is executed in a looping manner, if this is the selected gamemode.
        /// A game does not have to be started, for this function to be executed.
        /// 
        /// This function is bound to the HudUpdateEvent
        /// </summary>
        public virtual void Loop() {
            if (PlayerHudManager.HudManager == null)
                return;

            string modTag = "";

            if (!CheepsAmongUsMod.CreatorsRelease)
                modTag = $"{Functions.ColorPurple}{CheepsAmongUsMod.PluginName} {Functions.ColorBlue}v{CheepsAmongUsMod.PluginVersion} []by {Functions.ColorBlue}CheepYT[]\n";

            string topText =
                modTag +
            $"GameMode: {Functions.ColorPurple}{ GameModeName }[]" +
            GameModeSettingsAddition + "[]" +
            CheepsAmongUsMod.SettingsAddition + "[]";

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
        }

        /// <summary>
        /// Executed, once the game ends
        /// </summary>
        public virtual void OnEnd() { }

        /// <summary>
        /// Executed, once this gamemode is selected
        /// </summary>
        public virtual void OnSelect() { }

        /// <summary>
        /// Executed, once a lobby behaviour is started
        /// </summary>
        public virtual void ResetValues() {
            hasStarted = false;
        }

        /// <summary>
        /// Executed when settings are synced
        /// </summary>
        public virtual void SyncSettings() { }

        public void Unregister()
        {
            if(AvailableGameModes.Contains(this))
                AvailableGameModes.Remove(this);
        }
    }

    public class GameModeManager
    {
        public static List<GameMode> ActiveGameModes = new List<GameMode>();

        public static GameMode Selected
        {
            get
            {
                return CheepsAmongUsMod.SelectedGameMode;
            }

            set
            {
                CheepsAmongUsMod.SelectedGameMode = value;
            }
        }

        internal static void Start()
        {
            #region ---------- Ping Text ----------
            PlayerHudManager.AppendedPingText =
                $"\nMode: {Functions.ColorPurple}{ Selected.GameModeName }[]\n" + Selected.ModNotice +
                $"{GameMode.AdvStr}";

            PlayerHudManager.IsPingTextCentered = true;
            #endregion

            #region ---------- Changed GameMode ----------
            RpcManager.RpcReceived += RpcManager_RpcReceived;
            #endregion

            #region -------------------- Game Mode Execution --------------------

            #region ---------- Game Started ----------
            GameStartedEvent.Listener += () =>
            {
                if(CheepsAmongUsMod.IsDecidingClient)
                    RpcManager.SendRpc((byte)CustomRpcCalls.ChangeGameMode, Selected.GameModeId);

                Selected.OnStart();

                foreach (var gm in ActiveGameModes)
                    gm.OnStart();
            };
            #endregion

            #region ---------- Set Infected ----------
            SetInfectedEvent.Listener += () =>
            {
                Selected.OnSetInfected();

                foreach (var gm in ActiveGameModes)
                    gm.OnSetInfected();
            };
            #endregion

            #region ---------- Game Running ----------
            HudUpdateEvent.Listener += () =>
            {
                Selected.Loop();

                foreach (var gm in ActiveGameModes)
                    gm.Loop();
            };
            #endregion

            #region ---------- Game Ended ----------
            GameEndedEvent.Listener += () =>
            {
                Selected.OnEnd();

                foreach (var gm in ActiveGameModes)
                    gm.OnEnd();
            };
            #endregion

            #region ---------- Reset Values ----------
            LobbyBehaviourStartedEvent.Listener += () =>
            {
                Selected.ResetValues();

                foreach (var gm in ActiveGameModes)
                    gm.ResetValues();
            };
            #endregion

            #region ---------- Sync Settings ----------
            SyncedSettingsEvent.Listener += () =>
            {
                RpcManager.SendRpc((byte)CustomRpcCalls.ChangeGameMode, Selected.GameModeId);

                Selected.SyncSettings();

                foreach (var gm in ActiveGameModes)
                    gm.SyncSettings();
            };
            #endregion

            #endregion
        }

        internal static void GameModeNumberOption_ValueChanged(object sender, CustomNumberOption.CustomNumberOptionEventArgs e)
        {
            var gamemode = GameMode.AvailableGameModes[e.NumberOption.Value];

            ChangeToGameMode(gamemode);
        }

        internal static void ChangeToGameMode(GameMode mode)
        {
            Selected = mode;
            Selected.OnSelect();

            PlayerHudManager.AppendedPingText =
                $"\nMode: {Functions.ColorPurple}{ Selected.GameModeName }[]\n" + Selected.ModNotice +
                $"{GameMode.AdvStr}";

            RpcManager.SendRpc((byte)CustomRpcCalls.ChangeGameMode, mode.GameModeId);
        }

        private static void RpcManager_RpcReceived(object sender, RpcEventArgs e)
        {
            if(e.Command == (byte)CustomRpcCalls.ChangeGameMode)
            {
                var gamemode = e.MessageReader.ReadByte();

                var select = GameMode.AvailableGameModes.Where(x => x.GameModeId == gamemode);

                if(select.Count() == 0)
                {
                    // GameMode not found/installed
                    Functions.ShowPopUp(
                        $"[42ecf5ff]----- [7a31f7ff] GameMode [42ecf5ff]-----[]\n" +
                        $"The host has changed the GameMode to one, you do not have installed (ID: {gamemode}). You will not be able to play with this mode."
                        );

                    PlayerHudManager.AppendedPingText =
                        $"\nMode: {Functions.ColorPurple}Unknown ({ gamemode })[]\n" +
                        $"{GameMode.AdvStr}";
                } else
                {
                    Selected = select.FirstOrDefault();
                    Selected.OnSelect();

                    PlayerHudManager.AppendedPingText =
                        $"\nMode: {Functions.ColorPurple}{ Selected.GameModeName }[]\n" + Selected.ModNotice +
                        $"{GameMode.AdvStr}";
                }
            }
        }
    }
}
