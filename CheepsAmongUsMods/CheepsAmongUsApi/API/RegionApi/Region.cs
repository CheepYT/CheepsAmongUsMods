using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region -------------------- Among Us Types --------------------
using RegionManagerObject = AOBNFCIHAJL;
using RegionObject = OIBMKGDLGOG;
using ServerObject = PLFDMKKDEMI;
#endregion

namespace CheepsAmongUsApi.API.RegionApi
{
    public class Region
    {
        /// <summary>
        /// The list of servers
        /// </summary>
        public List<Server> ServerList { get; }

        /// <summary>
        /// The Among Us region object
        /// </summary>
        public RegionObject AmongUsRegionObject { get; private set; }

        /// <summary>
        /// The name of the region
        /// </summary>
        public string RegionName { get; }

        /// <summary>
        /// The ping IP of the region
        /// </summary>
        public string RegionPingIP { get; }


        /// <summary>
        /// Server Region
        /// </summary>
        /// <param name="regionName">The name of the region</param>
        /// <param name="regionPingIp">The ping IP of the region</param>
        /// <param name="serverList">A list of servers to add to the region</param>
        public Region(string regionName, string regionPingIp, List<Server> serverList)
        {
            RegionName = regionName;
            RegionPingIP = regionPingIp;

            ServerList = serverList;

            UpdateAuRegionObject();
        }

        /// <summary>
        /// Server Region
        /// </summary>
        /// <param name="regionName">The name of the region</param>
        /// <param name="regionPingIp">The ping IP of the region</param>
        /// <param name="server">A server to add to the region</param>
        public Region(string regionName, string regionPingIp, Server server)
        {
            RegionName = regionName;
            RegionPingIP = regionPingIp;

            ServerList = new List<Server>();
            ServerList.Add(server);

            UpdateAuRegionObject();
        }

        /// <summary>
        /// Updates the Among Us Region Object
        /// </summary>
        public void UpdateAuRegionObject()
        {
            var region = new List<ServerObject>();

            foreach (var server in ServerList)
                region.Add(server.AmongUsServerObject);

            AmongUsRegionObject = new RegionObject(RegionName, RegionPingIP, region.ToArray());
        }

        /// <summary>
        /// Add a server region to the game
        /// </summary>
        public void AddRegion()
        {
            // Get Default Regions
            List<RegionObject> RegionList = new List<RegionObject>(RegionManagerObject.DefaultRegions);

            // Add Region
            RegionList.Add(AmongUsRegionObject);

            // Set Default Regions
            RegionManagerObject.DefaultRegions = RegionList.ToArray();
        }

        /// <summary>
        /// Insert a server region into the game
        /// </summary>
        /// <param name="index">Determines, where the region will show up in the region list</param>
        public void InsertRegion(int index)
        {
            // Get Default Regions
            List<RegionObject> RegionList = new List<RegionObject>(RegionManagerObject.DefaultRegions);

            // Insert Region
            RegionList.Insert(index, AmongUsRegionObject);

            // Set Default Regions
            RegionManagerObject.DefaultRegions = RegionList.ToArray();
        }
    }
}
