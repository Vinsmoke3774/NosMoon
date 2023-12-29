using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;

namespace OpenNos.Handler.World.Family
{
    public class RefreshFamilyHistoryPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public RefreshFamilyHistoryPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// fhis_cts packet
        /// </summary>
        /// <param name="fhistCtsPacket"></param>
        public void FamilyRefreshHist(FhistCtsPacket fhistCtsPacket) =>
            Session.SendPackets(Session.Character.GetFamilyHistory());
    }
}
