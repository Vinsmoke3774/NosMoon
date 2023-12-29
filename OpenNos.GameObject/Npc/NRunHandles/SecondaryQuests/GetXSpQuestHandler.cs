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
    [NRunHandler(NRunType.GetXSPQuest)]
    public class GetXSpQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GetXSpQuestHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (packet.Type == 2000 && Npc.NpcVNum == 932 && !Session.Character.Quests.Any(s => s.QuestId >= 2000 && s.QuestId <= 2007) // Pajama
                || packet.Type == 2030 && Npc.NpcVNum == 422 && !Session.Character.Quests.Any(s => s.QuestId >= 2030 && s.QuestId <= 2046)
                || packet.Type == 2048 && Npc.NpcVNum == 303 && !Session.Character.Quests.Any(s => s.QuestId >= 2048 && s.QuestId <= 2050))
            {
                Session.Character.AddQuest(packet.Type);
            }
        }
    }
}
