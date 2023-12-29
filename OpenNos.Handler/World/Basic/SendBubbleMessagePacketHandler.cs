using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.Handler.World.Basic
{
    public class SendBubbleMessagePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public SendBubbleMessagePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// csp packet
        /// </summary>
        /// <param name="cspPacket"></param>
        public void SendBubbleMessage(CspPacket cspPacket)
        {
            if (cspPacket.CharacterId == Session.Character.CharacterId && Session.Character.BubbleMessage != null)
            {
                Session.Character.MapInstance.Broadcast(Session.Character.GenerateBubbleMessagePacket());
            }
        }
    }
}
