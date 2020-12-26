using CheepsAmongUsApi.API.Customization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region -------------------- Among Us Types --------------------
using PlayerDataObj = EGLJNOMOGNP.DCJMABDDJCF;  // TypeDefIndex: 7183
using PlayerPrefab = FFGALNAPKCD;
#endregion

namespace CheepsAmongUsApi.API
{
    public class PlayerData
    {
        /// <summary>
        /// Player Controller API object
        /// </summary>
        public PlayerController PlayerController { get; }

        /// <summary>
        /// Player Data Object
        /// </summary>
        public PlayerDataObj PlayerDataObject { get { return PlayerController.PlayerControl.JLGGIOLCDFC; /* TypeDefIndex: 7533 */ } }

        /// <summary>
        /// Player Prefab object
        /// </summary>
        public PlayerPrefab PlayerPrefab { get { return PlayerDataObject.LAOEJKHLKAI; /* Properties */ } }

        /// <summary>
        /// Gets or sets the among us is impostor value
        /// </summary>
        public bool IsImpostor
        {
            get
            {
                return PlayerDataObject.DAPKNDBLKIA;    // 0x28
            }

            set
            {
                PlayerDataObject.DAPKNDBLKIA = value;   // 0x28
            }
        }

        public uint HatId
        {
            get
            {
                return PlayerDataObject.AFEJLMBMKCJ; // 0x18
            }

            set
            {
                PlayerDataObject.AFEJLMBMKCJ = value; // 0x18
            }
        }

        public HatType Hat
        {
            get
            {
                return (HatType) HatId;
            }

            set
            {
                HatId = (uint) value;
            }
        }

        /// <summary>
        /// Gets or sets player dead boolean
        /// </summary>
        public bool IsDead
        {
            get
            {
                return PlayerDataObject.DLPCKPBIJOE;     // 0x29
            }
            set
            {   
               PlayerDataObject.DLPCKPBIJOE = value;     // 0x29
            }
        }

        /// <summary>
        /// Gets or sets the player name
        /// </summary>
        public string PlayerName
        {
            get
            {
                return PlayerDataObject.EIGEKHDAKOH;     // 0xC
            }
            set
            {
                PlayerDataObject.EIGEKHDAKOH = value;    // 0xC
            }   
        }

        /// <summary>
        /// Creates a player data object
        /// </summary>
        /// <param name="controller">Player Controller Object</param>
        public PlayerData(PlayerController controller)
        {
            PlayerController = controller;
        }
    }
}
