using CheepsAmongUsApi.API;
using HarmonyLib;
using Hazel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AmongUsClient = FMLLKEACGIO;
using PlayerControl = FFGALNAPKCD;

namespace CheepsAmongUsMod.API
{
    public class RpcManager
    {
        public static event EventHandler<RpcEventArgs> RpcReceived;

        /// <summary>
        /// Sends an Rpc Message with a single byte value
        /// </summary>
        /// <param name="cmd">Used to identify Rpc call</param>
        /// <param name="val">Byte Value</param>
        public static void SendRpc(byte cmd, byte val)
        {
            SendRpc(cmd, new byte[] { val });
        }

        /// <summary>
        /// Starts an rpc message
        /// </summary>
        /// <param name="cmd">Used to identify Rpc call</param>
        /// <returns>The message writer instance</returns>
        public static MessageWriter StartRpc(byte cmd)
        {
            return AmongUsClient.Instance.StartRpc(PlayerController.LocalPlayer.NetId, cmd);
        }

        /// <summary>
        /// Sends an Rpc Message
        /// </summary>
        /// <param name="cmd">Used to identify Rpc call</param>
        /// <param name="vals">Array of byte values</param>
        public static void SendRpc(byte cmd, byte[] vals)
        {
            var writer = AmongUsClient.Instance.StartRpc(PlayerController.LocalPlayer.NetId, cmd);
            foreach (var val in vals)
                writer.Write(val);

            writer.EndMessage();
        }

        #region -------------------- Patch PlayerControl Handle Rpc --------------------
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        public static class Patch_PlayerControl_HandleRpc
        {
            public static void Prefix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                RpcReceived?.Invoke(null, new RpcEventArgs() { Command = callId, MessageReader = reader });
            }
        }
        #endregion
    }

    public class RpcEventArgs : EventArgs
    {
        public byte Command { get; set; }
        public MessageReader MessageReader { get; set; }
    }

    /// <summary>
    /// Mainly used internally
    /// </summary>
    public enum CustomRpcCalls : byte
    {
        CheepsAmongUsModData = 59,
        ChangeGameMode = 60
    }
}
