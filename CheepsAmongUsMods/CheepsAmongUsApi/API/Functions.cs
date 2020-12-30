using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using GameObject = FFGALNAPKCD;
using GameOptionsObject = KMOGFLPJLLK;
using RpcClass = FMLLKEACGIO;

namespace CheepsAmongUsApi.API
{
    public class Functions
    {
        public const string ResetColor = "[]";
        public const string ColorWhite = "[ffffffff]";
        public const string ColorBlack = "[000000ff]";
        public const string ColorRed = "[ff0000ff]";
        public const string ColorGreen = "[169116ff]";
        public const string ColorBlue = "[0400ffff]";
        public const string ColorYellow = "[f5e90cff]";
        public const string ColorPurple = "[a600ffff]";
        public const string ColorCyan = "[00fff2ff]";
        public const string ColorPink = "[e34dd4ff]";
        public const string ColorOrange = "[ff8c00ff]";
        public const string ColorBrown = "[8c5108ff]";
        public const string ColorLime = "[1eff00ff]";

        public static int GetUnixTime()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static MessageWriter StartRpcImmediately(uint netId, byte rpcCall, SendOption option, int unknown = -1)
        {
            return RpcClass.Instance.StartRpcImmediately(netId, rpcCall, option, unknown);
        }

        public static void FinishRpcImmediately(MessageWriter writer)
        {
            RpcClass.Instance.FinishRpcImmediately(writer);
        }

        public static GameOptionsObject GameOptions
        {
            get
            {
                return GameObject.GameOptions;
            }
        }

        public static bool IsInGame
        { 
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Current Mouse Position
        /// </summary>
        public static Vector2 MousePosition
        {
            get
            {
                return Input.mousePosition;
            }
        }

        public static bool GetKeyDown(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public static bool GetKeyUp(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        public static bool GetKey(KeyCode key)
        {
            return Input.GetKey(key);
        }

        public static Vector2 GetMouseScrollDelta()
        {
            return Input.mouseScrollDelta;
        }

        public static void ShowPopUp(string text)
        {
            PlayerHudManager.HudManager.ShowPopUp(text);
        }
    }
}
