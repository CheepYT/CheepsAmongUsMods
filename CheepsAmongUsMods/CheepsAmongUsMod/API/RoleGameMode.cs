using CheepsAmongUsApi.API.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheepsAmongUsMod.API
{
    public class RoleGameMode
    {
        public static List<RoleGameMode> AllGameModes = new List<RoleGameMode>();

        public List<RolePlayer> AllRolePlayers = new List<RolePlayer>();

        public RoleGameMode()
        {
            AllGameModes.Add(this);
        }

        public bool IsInGame
        {
            get
            {
                return GameModeManager.Selected.IsInGame;
            }
        }

        /// <summary>
        /// Executed, once the game starts
        /// </summary>
        public virtual void OnStart() { }

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
        public virtual void Loop() { }


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
            AllRolePlayers.Clear();
        }

        /// <summary>
        /// Executed when settings are synced
        /// </summary>
        public virtual void SyncSettings() { }
    }

    public class RoleGameModeManager
    {
        internal static void Start()
        {

            #region -------------------- Game Mode Execution --------------------

            #region ---------- Game Started ----------
            GameStartedEvent.Listener += () =>
            {
                foreach(var gm in RoleGameMode.AllGameModes)
                    gm.OnStart();
            };
            #endregion

            #region ---------- Set Infected ----------
            SetInfectedEvent.Listener += () =>
            {
                foreach (var gm in RoleGameMode.AllGameModes)
                    gm.OnSetInfected();
            };
            #endregion

            #region ---------- Game Running ----------
            HudUpdateEvent.Listener += () =>
            {
                foreach (var gm in RoleGameMode.AllGameModes)
                    gm.Loop();
            };
            #endregion

            #region ---------- Game Ended ----------
            GameEndedEvent.Listener += () =>
            {
                foreach (var gm in RoleGameMode.AllGameModes)
                    gm.OnEnd();
            };
            #endregion

            #region ---------- Reset Values ----------
            LobbyBehaviourStartedEvent.Listener += () =>
            {
                foreach (var gm in RoleGameMode.AllGameModes)
                    gm.ResetValues();
            };
            #endregion

            #region ---------- Sync Settings ----------
            SyncedSettingsEvent.Listener += () =>
            {
                foreach (var gm in RoleGameMode.AllGameModes)
                    gm.SyncSettings();
            };
            #endregion

            #endregion
        }
    }
}
