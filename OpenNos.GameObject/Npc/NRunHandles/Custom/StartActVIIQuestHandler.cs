using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.StartActVIIQuest)]
    public class StartActVIIQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public StartActVIIQuestHandler(ClientSession session) : base(session)
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
            return;
            Session.Character.AddQuest(6500);
        }
    }
}
