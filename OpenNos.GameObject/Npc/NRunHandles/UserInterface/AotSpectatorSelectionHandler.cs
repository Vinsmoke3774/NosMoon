using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.UserInterface
{
    [NRunHandler(NRunType.ArenaEnterAsSpectatorSelection)]
    public class AotSpectatorSelectionHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public AotSpectatorSelectionHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Session.SendPacket("taw_open");
        }
    }
}
