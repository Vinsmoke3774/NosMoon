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

namespace OpenNos.GameObject.Npc.NRunHandles.Teleports
{
    [NRunHandler(NRunType.GoToXTimeSpace)]
    public class GoToXTimeSpace : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GoToXTimeSpace(ClientSession session) : base(session)
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
            if (Session.Character.Quests.Any(s => s.Quest.DialogNpcVNum == Npc.NpcVNum && s.Quest.QuestObjectives.Any(o => o.SpecialData == packet.Type)))
            {
                if (ServerManager.Instance.TimeSpaces.FirstOrDefault(s => s.QuestTimeSpaceId == packet.Type) is GameObject.ScriptedInstance timeSpace)
                {
                    Session.Character.EnterInstance(timeSpace);
                }
            }
        }
    }
}
