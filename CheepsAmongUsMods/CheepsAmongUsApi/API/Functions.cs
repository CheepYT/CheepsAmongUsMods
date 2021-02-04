using Hazel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;

using GameObject = FFGALNAPKCD;
using GameOptionsObject = KMOGFLPJLLK;
using RpcClass = FMLLKEACGIO;
using ShipStatusClass = HLBNNHFCNAJ;
using EndGameReason = AIMMJPEOPEC;

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

        internal delegate bool d_LoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        internal static d_LoadImage iCall_LoadImage;

        public static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            if (iCall_LoadImage == null)
                iCall_LoadImage = IL2CPP.ResolveICall<d_LoadImage>("UnityEngine.ImageConversion::LoadImage");

            var il2cppArray = (Il2CppStructArray<byte>)data;

            return iCall_LoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        public static void RpcEndGame(EndGameReason reason)
        {
            ShipStatusClass.PLBGOMIEONF(reason, false);
        }

        public static int Ping
        {
            get
            {
                return RpcClass.Instance.DGAKPKLMIEI; // Properties 2
            }
        }

        public static Texture2D LoadTextureFromFile(string filepath)
        {
            try
            {
                using (var stream = new StreamReader(filepath))
                {
                    var texture = stream.BaseStream;

                    Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);

                    byte[] textureArr = new byte[texture.Length];
                    texture.Read(textureArr, 0, (int)texture.Length);
                    LoadImage(tex, textureArr, false);

                    return tex;
                }
            }
            catch (Exception e)
            {
                CheepsAmongUs._logger.LogInfo("An error occured, while attempting to load a texture from a file.");
                throw e;
            }
        }

        public static Vector2 PositionFromDistance(float distance, float angle, Vector2 init)
        {
            var dist = distance;

            var x = dist * Mathf.Cos(angle / (180 / (float)Math.PI));
            var y = dist * Mathf.Sin(angle / (180 / (float)Math.PI));
            var newPosition = init;
            newPosition.x += x;
            newPosition.y += y;
            return newPosition;
        }

        public static Sprite LoadSpriteFromFile(string filepath)
        {
            var tex = LoadTextureFromFile(filepath);

            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        public static Sprite LoadSpriteFromAssemblyResource(Assembly assembly, string resource)
        {
            var tex = LoadTextureFromAssemblyResource(assembly, resource);

            return Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        public static Texture2D LoadTextureFromAssemblyResource(Assembly assembly, string resource)
        {
            try
            {
                var texture = assembly.GetManifestResourceStream(resource);

                Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);

                byte[] hatTexture = new byte[texture.Length];
                texture.Read(hatTexture, 0, (int)texture.Length);
                LoadImage(tex, hatTexture, false);

                return tex;
            }
            catch (Exception e)
            {
                CheepsAmongUs._logger.LogInfo("An error occured, while attempting to load a texture from an assembly resource.");
                throw e;
            }
        }


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

        public static Vector3 MouseWorldPosition
        {
            get
            {
                return Camera.main.ScreenToWorldPoint(MousePosition);
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
