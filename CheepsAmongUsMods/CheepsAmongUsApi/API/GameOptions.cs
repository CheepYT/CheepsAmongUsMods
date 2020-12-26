using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheepsAmongUsApi.API.Enumerations;

using GameObject = FFGALNAPKCD;
using GameOptionsObject = KMOGFLPJLLK;
using TaskBarUpdateEnum = GLOLCBDMNHG;
using LanguageEnum = IJAFEDPACBD;

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

        public static MapType Map
        {
            get
            {
                return (MapType)GameOptionsObject.PNGONPDAFPF;
            }

            set
            {
                GameOptionsObject.PNGONPDAFPF = (byte)value;
            }
        }

        public static int ShortTasks // 0x2C
        {
            get
            {
                return GameOptionsObject.DNIEHDEAMPH;
            }

            set
            {
                GameOptionsObject.DNIEHDEAMPH = value;
            }
        }

        public static byte LongTasks // Fields
        {
            get
            {
                return GameOptionsObject.GBJBJFEGKBN;
            }

            set
            {
                GameOptionsObject.GBJBJFEGKBN = value;
            }
        }

        public static int CommonTasks // 0x24
        {
            get
            {
                return GameOptionsObject.BNJBANDKLOJ;
            }

            set
            {
                GameOptionsObject.BNJBANDKLOJ = value;
            }
        }

        public static KillDistance KillDistance // 0x40
        {
            get
            {
                return (KillDistance)GameOptionsObject.DLIBONBKPKL;
            }

            set
            {
                GameOptionsObject.DLIBONBKPKL = (int)value;
            }
        }

        public static bool ConfirmEjects // 0x4C
        {
            get
            {
                return GameOptionsObject.HGOMOAAPHNJ;
            }

            set
            {
                GameOptionsObject.HGOMOAAPHNJ = value;
            }
        }

        public static int EmergencyMeetings // 0x30
        {
            get
            {
                return GameOptionsObject.OEGKGKCNMKD;
            }

            set
            {
                GameOptionsObject.OEGKGKCNMKD = value;
            }
        }

        public static int ImpostorCount // 0x38
        {
            get
            {
                return GameOptionsObject.KDEGPDECMHF;
            }

            set
            {
                GameOptionsObject.KDEGPDECMHF = value;
            }
        }

        public static int MaxPlayers // 0x8
        {
            get
            {
                return GameOptionsObject.NCJGOCGPJDO;
            }

            set
            {
                GameOptionsObject.NCJGOCGPJDO = value;
            }
        }

        public static LanguageEnum Language // 0xC
        {
            get
            {
                return GameOptionsObject.IPPFPEALJIA;
            }

            set
            {
                GameOptionsObject.IPPFPEALJIA = value;
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
