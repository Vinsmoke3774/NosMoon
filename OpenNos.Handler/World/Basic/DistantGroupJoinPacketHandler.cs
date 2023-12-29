using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class DistantGroupJoinPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public DistantGroupJoinPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// $pinv packet
        /// </summary>
        /// <param name="pinvPacket"></param>
        public void PinvGroupJoin(PinvPacket pinvPacket)
        {
            if (pinvPacket.CharacterName != null && ServerManager.Instance.GetSessionByCharacterName(pinvPacket.CharacterName) is ClientSession receiverSession)
            {
                new JoinGroupPacketHandler(Session).GroupJoin(new PJoinPacket { RequestType = GroupRequestType.Requested, CharacterId = receiverSession.Character.CharacterId });
            }
        }
    }
}
