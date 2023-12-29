using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.Handler.World.Family
{
    public class FamilyListPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public FamilyListPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// glist packet
        /// </summary>
        /// <param name="gListPacket"></param>
        public void FamilyList(GListPacket gListPacket)
        {
            if (Session.Character.Family != null && Session.Character.FamilyCharacter != null && gListPacket.Type == 2)
            {
                Session.SendPacket(Session.Character.GenerateGInfo());
                Session.SendPacket(Session.Character.GenerateFamilyMember());
                Session.SendPacket(Session.Character.GenerateFamilyMemberMessage());
                Session.SendPacket(Session.Character.GenerateFamilyMemberExp());
            }
        }
    }
}
