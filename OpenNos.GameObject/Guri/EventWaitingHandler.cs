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
    [GuriHandler(GuriType.EventWaiting)]
    public class EventWaitingHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public EventWaitingHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            Session.Character.IsWaitingForEvent |= ServerManager.Instance.EventInWaiting;
        }
    }
}
