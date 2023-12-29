using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.SecondaryQuests
{
    [NRunHandler(NRunType.GetQuestListFromNPC)]
    public class GetBurningSwordQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GetBurningSwordQuestHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Session.Character.AddQuest(5478, true);
        }
    }
}
