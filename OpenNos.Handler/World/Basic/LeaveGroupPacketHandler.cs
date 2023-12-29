using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class LeaveGroupPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public LeaveGroupPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// pleave packet
        /// </summary>
        /// <param name="pleavePacket"></param>
        public void GroupLeave(PLeavePacket pleavePacket) => ServerManager.Instance.GroupLeave(Session);
    }
}
