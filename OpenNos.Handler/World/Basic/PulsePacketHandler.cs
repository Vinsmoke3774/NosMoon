using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;
using OpenNos.Master.Library.Client;

namespace OpenNos.Handler.World.Basic
{
    public class PulsePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public PulsePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// pulse packet
        /// </summary>
        /// <param name="pulsepacket"></param>
        public void Pulse(PulsePacket pulsepacket)
        {
            if (Session.Character.LastPulse.AddMilliseconds(80000) >= DateTime.Now
                && DateTime.Now >= Session.Character.LastPulse.AddMilliseconds(40000))
            {
                Session.Character.LastPulse = DateTime.Now;
            }
            else
            {
                Session.Disconnect();
            }

            Session.Character.MuteMessage();
            Session.Character.DeleteTimeout();
            CommunicationServiceClient.Instance.PulseAccount(Session.Account.AccountId);
        }
    }
}
