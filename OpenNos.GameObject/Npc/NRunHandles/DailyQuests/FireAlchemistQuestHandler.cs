using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.DailyQuests
{
    [NRunHandler(NRunType.GetDailyQuestFireAlchemistHeatResistancePotion)]
    public class FireAlchemistQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public FireAlchemistQuestHandler(ClientSession session) : base(session)
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
            Session.Character.AddQuest(5914);
        }
    }
}
