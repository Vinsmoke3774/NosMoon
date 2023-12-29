using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Family
{
    public class UiChangeRankPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public UiChangeRankPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// frank_cts packet
        /// </summary>
        /// <param name="frankCtsPacket"></param>
        public void FamilyRank(FrankCtsPacket frankCtsPacket) =>
            Session.SendPacket(UserInterfaceHelper.GenerateFrank(frankCtsPacket.Type));
    }
}
