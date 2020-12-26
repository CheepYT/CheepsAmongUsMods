using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region -------------------- Among Us Types --------------------
using ServerObject = PLFDMKKDEMI;
#endregion

namespace CheepsAmongUsApi.API.RegionApi
{
    public class Server
    {
        /// <summary>
        /// The Among Us server object
        /// </summary>
        public readonly ServerObject AmongUsServerObject;

        /// <summary>
        /// The server name
        /// </summary>
        public string ServerName { get; }

        /// <summary>
        /// The servers IP address
        /// </summary>
        public string ServerIP { get; }

        /// <summary>
        /// The servers port (default is 22023)
        /// </summary>
        public ushort ServerPort { get; }

        /// <summary>
        /// Server
        /// </summary>
        /// <param name="serverName">The name of the server</param>
        /// <param name="serverIp">The servers IP address</param>
        /// <param name="serverPort">The servers port (default is 22023)</param>
        public Server(string serverName, string serverIp, ushort serverPort)
        {
            // Create a new Among Us server object
            AmongUsServerObject = new ServerObject(serverName, serverIp, serverPort);

            ServerName = serverName;
            ServerIP = serverIp;
            ServerPort = serverPort;
        }
    }
}
