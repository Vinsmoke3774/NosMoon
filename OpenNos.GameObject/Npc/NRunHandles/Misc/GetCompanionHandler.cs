using System.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.Misc
{
    [NRunHandler(NRunType.GetXCompanion)]
    public class GetCompanionHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public GetCompanionHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            NpcMonster heldMonster = ServerManager.GetNpcMonster((short)packet.Type);
            if (heldMonster != null && !Session.Character.Mates.Any(m => m.NpcMonsterVNum == heldMonster.NpcMonsterVNum && !m.IsTemporalMate) && Session.Character.Mates?.ToList().FirstOrDefault(s => s.NpcMonsterVNum == heldMonster.NpcMonsterVNum && s.IsTemporalMate && s.IsTsReward) is Mate partnerToReceive)
            {
                Session.Character.RemoveTemporalMates();
                Mate partner = new Mate(Session.Character, heldMonster, heldMonster.Level, MateType.Partner)
                {
                    Experience = partnerToReceive.Experience
                };
                if (!Session.Character.Mates.Any(s => s.MateType == MateType.Partner && s.IsTeamMember))
                {
                    partner.IsTeamMember = true;
                }
                Session.Character.AddPet(partner);
            }
        }
    }
}
