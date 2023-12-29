using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets.Family;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Family
{
    public class CmdFamilyGoldPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CmdFamilyGoldPacketHandler(ClientSession session) => Session = session;

        public void FamilyGold(FamilyGoldPacket familyGold)
        {
            if(Session.Character.Family == null)
            {
                return;
            }
                Session.SendPacket(Session.Character.GenerateSay($"Your family has currently saved {Session.Character.Family.FamilyGold} gold.", 12));
                return;
            
        }
    }
}
