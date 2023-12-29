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
    public class UiTodayMessagePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public UiTodayMessagePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// today_cts packet
        /// </summary>
        /// <param name="todayPacket"></param>
        public void FamilyChangeMessage(TodayPacket todayPacket) => Session.SendPacket("today_stc");
    }
}
