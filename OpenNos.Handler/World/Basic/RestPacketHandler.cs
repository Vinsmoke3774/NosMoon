using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;

namespace OpenNos.Handler.World.Basic
{
    public class RestPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public RestPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// rest packet
        /// </summary>
        /// <param name="sitpacket"></param>
        public void Rest(SitPacket sitpacket)
        {
            if (Session.Character.MeditationDictionary.Count != 0)
            {
                Session.Character.MeditationDictionary.Clear();
            }

            sitpacket.Users?.ForEach(u =>
            {
                if (u.UserType == 1)
                {
                    Session.Character.Rest();
                }
                else
                {
                    Session.CurrentMapInstance.Broadcast(Session.Character.Mates
                        .Find(s => s.MateTransportId == (int)u.UserId)?.GenerateRest(sitpacket.Users[0] != u));
                }
            });
        }
    }
}
