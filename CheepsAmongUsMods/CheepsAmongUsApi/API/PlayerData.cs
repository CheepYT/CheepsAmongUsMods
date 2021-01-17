using CheepsAmongUsApi.API.Enumerations;
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
        public PlayerDataObj PlayerDataObject { get { return PlayerController.PlayerControl.NDGFFHMFGIG; /* 0x34 */ } }

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

        public byte PlayerId
        {
            get
            {
                return PlayerDataObject.JKOMCOJCAID; // 0x8
            }
        }

        public byte ColorId
        {
            get
            {
                return PlayerDataObject.EHAHBDFODKC; // 0x10
            }

            set
            {
                PlayerDataObject.EHAHBDFODKC = value; // 0x10
            }
        }

        public ColorType Color
        {
            get
            {
                return (ColorType)ColorId;
            }
            set
            {
                ColorId = (byte)value;
            }
        }

        public uint SkinId
        {
            get
            {
                return PlayerDataObject.HPAMBHFDLEH; // 0x1C
            }

            set
            {
                PlayerDataObject.HPAMBHFDLEH = value; // 0x1C
            }
        }

        public SkinType Skin
        {
            get
            {
                return (SkinType)SkinId;
            }

            set
            {
                SkinId = (uint)value;
            }
        }

        public uint PetId
        {
            get
            {
                return PlayerDataObject.AJIBCNMKNPM; // 0x18
            }
            set
            {
                PlayerDataObject.AJIBCNMKNPM = value; // 0x18
            }
        }

        public PetType Pet
        {
            get
            {
                return (PetType)PetId;
            }

            set
            {
                PetId = (uint)value;
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

        public PlayerData(PlayerDataObj data)
        {
            PlayerController = new PlayerController(data.LAOEJKHLKAI); // Properties
        }
    }
}
