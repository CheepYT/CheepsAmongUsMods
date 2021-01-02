using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using CheepsAmongUsApi.API.Enumerations;

#region -------------------- Among Us Types --------------------
using PlayerControl = FFGALNAPKCD;   // TypeDefIndex: 7533
using Task = PILBGHDHJLH;            // TypeDefIndex: 7707
using TaskObj = EGLJNOMOGNP.CAAACHLJJNE;
using DeathReasonClass = DBLJKMDLJIF;
#endregion

namespace CheepsAmongUsApi.API
{
    public class PlayerController
    {
        public const float OffsetTruePositionX = 0;
        public const float OffsetTruePositionY = 0.366667f;

        public static Vector2 TruePositionOffset = new Vector2(OffsetTruePositionX, OffsetTruePositionY);

        /// <summary>
        /// Among Us PlayerControl object
        /// </summary>
        public PlayerControl PlayerControl { get; }

        /// <summary>
        /// Gets the local player and converts it to a PlayerController
        /// </summary>
        /// <returns></returns>
        public static PlayerController GetLocalPlayer()
        {
            return new PlayerController(PlayerControl.LocalPlayer);
        }

        /// <summary>
        /// Gets all player controls and converts them to a list of PlayerController
        /// </summary>
        /// <returns></returns>
        public static List<PlayerController> GetAllPlayers()
        {
            List<PlayerController> toRt = new List<PlayerController>();

            foreach (var playerCtrl in PlayerControl.AllPlayerControls)
                toRt.Add(new PlayerController(playerCtrl));
            
            return toRt;
        }

        /// <summary>
        /// Compares two PlayerController NetIds
        /// </summary>
        /// <param name="obj">The other object</param>
        /// <returns></returns>
        public bool Equals(PlayerController obj)
        {
            return NetId == obj.NetId;
        }

        /// <summary>
        /// Returns true, if the player controller is the local one
        /// </summary>
        /// <returns>true, if player controller is the local one</returns>
        public bool AmPlayerController()
        {
            return Equals(GetLocalPlayer());
        }

        /// <summary>
        /// Sets a players tasks
        /// </summary>
        /// <param name="tasks">A list of AU Task Objects</param>
        public void SetTasks(Il2CppSystem.Collections.Generic.List<TaskObj> tasks)
        {
            PlayerControl.SetTasks(tasks);
        }

        /// <summary>
        /// Clears a players tasks
        /// </summary>
        public void ClearTasks()
        {
            SetTasks(new Il2CppSystem.Collections.Generic.List<TaskObj>());
        }

        /// <summary>
        /// Kills the player
        /// </summary>
        /// <param name="reason">The reason to die</param>
        public void Die(DeathReason reason = DeathReason.Kill)
        {
            PlayerControl.Die((DeathReasonClass) reason);
        }

        public void Revive()
        {
            PlayerControl.Revive();
        }

        /// <summary>
        /// Moveable bool
        /// </summary>
        public bool Moveable
        {
            get
            {
                return PlayerControl.moveable;
            }

            set
            {
                PlayerControl.moveable = value;
            }
        }

        /// <summary>
        /// The players true position
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return PlayerControl.GetTruePosition() + TruePositionOffset;
            }
        }

        /// <summary>
        /// Player data among us object
        /// </summary>
        public PlayerData PlayerData { get { return new PlayerData(this); } }

        /// <summary>
        /// Network ID
        /// </summary>
        public uint NetId { get { return PlayerControl.NetId; } set { PlayerControl.NetId = value; } }

        /// <summary>
        /// Gets or sets player task objects
        /// </summary>
        public Il2CppSystem.Collections.Generic.List<Task> PlayerTaskObjects
        {
            get
            {
                return PlayerControl.myTasks;    // 0x74
            }
            set
            {
                PlayerControl.myTasks =  value;  // 0x74
            }
        }

        /// <summary>
        /// Sends a chat message from the player
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns></returns>
        public bool RpcSendChat(string message)
        {
            return PlayerControl.RpcSendChat(message);
        }

        /// <summary>
        /// Set the players opacity (hat bugs a bit)
        /// </summary>
        /// <param name="opacity">Opacity value from 0 - 1</param>
        public void SetOpacity(float opacity)
        {
            var toSetColor = new Color(1, 1, 1, opacity);

            PlayerControl.GetComponent<SpriteRenderer>().color = toSetColor;

            PlayerControl.HatRenderer.FrontLayer.color = toSetColor;
            PlayerControl.HatRenderer.BackLayer.color = toSetColor;
            PlayerControl.HatRenderer.HHJCOBKGBFF = toSetColor;          // TypeDefIndex: 7525
            PlayerControl.MyPhysics.Skin.layer.color = toSetColor;
            PlayerControl.nameText.Color = toSetColor;
        }

        /// <summary>
        /// Player speed
        /// </summary>
        public float Speed
        {
            get
            {
                return PlayerControl.MyPhysics.Speed;
            }

            set
            {
                PlayerControl.MyPhysics.Speed = value;
            }
        }

        /// <summary>
        /// Sets player opacity to 1/0
        /// </summary>
        public bool IsVisible
        {
            set
            {
                //if (value && PrevHat.ContainsKey(NetId))
                    //PlayerControl.SetHat((uint)PrevHat[NetId], (int)PlayerData.HatId); // Set previous hat
                //else
               // {
                    //PrevHat[NetId] = PlayerData.Hat; // Save previous hat

                    //PlayerControl.SetHat(0, (int)PlayerData.HatId);   // Remove hat, because for some reason, it wont update opacity correctly
               // }

                SetOpacity(value ? 1 : 0);
            }
        }

        public void RpcSetHat(HatType hat)
        {
            PlayerControl.RpcSetHat((uint)hat);
        }

        public void RpcSetColor(ColorType color)
        {
            PlayerControl.RpcSetColor((byte)color);
        }

        public void RpcSetName(string name)
        {
            PlayerControl.RpcSetName(name);
        }

        public void RpcSetPet(PetType pet)
        {
            PlayerControl.RpcSetPet((uint)pet);
        }

        public void RpcSetSkin(SkinType skin)
        {
            PlayerControl.RpcSetSkin((uint)skin);
        }

        /// <summary>
        /// Snaps the player to a position
        /// </summary>
        /// <param name="position">The position</param>
        public void RpcSnapTo(Vector2 position)
        {
            PlayerControl.NetTransform.RpcSnapTo(position);
        }

        /// <summary>
        /// The player id
        /// </summary>
        public byte PlayerId
        {
            get
            {
                return PlayerControl.PlayerId;
            }

            set
            {
                PlayerControl.PlayerId = value;
            }
        }

        /// <summary>
        /// Enables/Disables player collider
        /// </summary>
        public bool HasCollision
        {
            get
            {
                return PlayerControl.Collider.enabled;
            }

            set
            {
                PlayerControl.Collider.enabled = value;
            }
        }

        /// <summary>
        /// Instance of PlayerController
        /// </summary>
        /// <param name="playerControl">Among Us PlayerControl object</param>
        public PlayerController(PlayerControl playerControl)
        {
            PlayerControl = playerControl;
        }

        /// <summary>
        /// Returns the player controller from a name
        /// </summary>
        /// <param name="name">The players name</param>
        public static PlayerController FromName(string name)
        {
            foreach (var ctrls in GetAllPlayers())
                if (ctrls.PlayerData.PlayerName == name)
                    return ctrls;

            return null;
        }

        /// <summary>
        /// Returns the player controller from a network id
        /// </summary>
        /// <param name="netId">The players net id</param>
        public static PlayerController FromNetId(uint netId)
        {
            foreach (var ctrls in GetAllPlayers())
                if (ctrls.NetId == netId)
                    return ctrls;

            return null;
        }

        public static PlayerController FromPlayerId(byte id)
        {
            foreach (var ctrl in GetAllPlayers())
                if (ctrl.PlayerId == id)
                    return ctrl;

            return null;
        }
    }
}
