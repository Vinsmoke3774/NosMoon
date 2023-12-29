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
    [NRunHandler(NRunType.GetXQuestFromDialog2Quest)]
    public class GetXQuestFromDialogHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GetXQuestFromDialogHandler(ClientSession session) : base(session)
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
            if (Session.Character.Quests.Any(s => s.Quest.QuestType == (int)QuestType.Dialog2 && s.Quest.QuestObjectives.Any(b => b.Data == Npc.NpcVNum)))
            {
                Session.Character.AddQuest(packet.Type);
                Session.Character.IncrementQuests(QuestType.Dialog2, Npc.NpcVNum);
            }
        }
    }
}
