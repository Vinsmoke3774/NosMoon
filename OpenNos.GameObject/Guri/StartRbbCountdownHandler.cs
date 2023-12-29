using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.StartRbbCountdown)]
    public class StartRbbCountdownHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public StartRbbCountdownHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (ServerManager.Instance.EventInWaiting == true && Session.Character.IsWaitingForEvent == false)
            {
                Session.SendPacket("bsinfo 0 7 30 0");
                Session.SendPacket("esf 2");
                Session.Character.IsWaitingForEvent = true;
            }
        }
    }
}
