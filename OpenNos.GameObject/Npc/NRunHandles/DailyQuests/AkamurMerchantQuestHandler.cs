using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.DailyQuests
{
    [NRunHandler(NRunType.GetDailyQuestAkamurMerchantMilitaryEngineerEliminateMonsters)]
    public class AkamurMerchantQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public AkamurMerchantQuestHandler(ClientSession session) : base(session)
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
            Session.Character.AddQuest(5908);
        }
    }
}
