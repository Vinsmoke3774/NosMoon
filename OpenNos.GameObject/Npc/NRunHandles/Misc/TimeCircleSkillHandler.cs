using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.GameObject.Npc.NRunHandles.Misc
{
    [NRunHandler(NRunType.TimeCircleSkill)]
    public class TimeCircleSkillHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TimeCircleSkillHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            if (Session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
            {
                return;
            }
            Session.SendPacket(Session.Character.GenerateNpcDialog(17));
        }
    }
}
