using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GameObject = FFGALNAPKCD;
using GameOptionsObject = KMOGFLPJLLK;
using TaskBarUpdateEnum = GLOLCBDMNHG;

namespace CheepsAmongUsApi.API
{
    public class GameOptions
    {
        public static GameOptionsObject GameOptionsObject
        {
            get
            {
                return GameObject.GameOptions;
            }
        }

        public static bool IsDefaults // 0x4E
        {
            get
            {
                return GameOptionsObject.AGGKAFILPGD;
            }

            set
            {
                GameOptionsObject.AGGKAFILPGD = value;
            }
        }

        public static bool ConfirmEjects // 0x4D
        {
            get
            {
                return GameOptionsObject.EPMDHIPBJPF;
            }

            set
            {
                GameOptionsObject.EPMDHIPBJPF = value;
            }
        }

        public static float ImpostorVision // 0x1C
        {
            get
            {
                return GameOptionsObject.GFDCMFFAAGI;
            }
            set
            {
                GameOptionsObject.GFDCMFFAAGI = value;
            }
        }

        public static float CrewmateVision // 0x18
        {
            get
            {
                return GameOptionsObject.POMBOFGMLEN;
            }
            set
            {
                GameOptionsObject.POMBOFGMLEN = value;
            }
        }

        public static int VotingTime // 0x48
        {
            get
            {
                return GameOptionsObject.HLDHIMANCIL;
            }

            set
            {
                GameOptionsObject.HLDHIMANCIL = value;
            }
        }

        public static int DiscussionTime // 0x44
        {
            get
            {
                return GameOptionsObject.PCCONFJBDJL;
            }

            set
            {
                GameOptionsObject.PCCONFJBDJL = value;
            }
        }

        public static int EmergencyCooldown // 0x34
        {
            get
            {
                return GameOptionsObject.LOAELKJHIMH;
            }

            set
            {
                GameOptionsObject.LOAELKJHIMH = value;
            }
        }

        public static TaskBarUpdateEnum TaskBarUpdates // 0x50
        {
            get
            {
                return GameOptionsObject.GLOLCBDMNHG;
            }

            set
            {
                GameOptionsObject.GLOLCBDMNHG = value;
            }
        }

        public static float KillCooldown // 0x20
        {
            get
            {
                return GameOptionsObject.IGHCIKIDAMO;
            }

            set
            {
                GameOptionsObject.IGHCIKIDAMO = value;
            }
        }

        public static float PlayerSpeed // 0x14
        {
            get
            {
                return GameOptionsObject.KOHHMIGDLCP;
            }

            set
            {
                GameOptionsObject.KOHHMIGDLCP = value;
            }
        }

    }
}
