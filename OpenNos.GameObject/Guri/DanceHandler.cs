using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.DanceAnimation)]
    public class DanceHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public DanceHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            Session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(2, 1, Session.Character.CharacterId), Session.Character.PositionX, Session.Character.PositionY);
        }
    }
}
