using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.UserInterface
{
    [NRunHandler(NRunType.ProbabilityUIs)]
    public class OpenProbabilityWindowHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public OpenProbabilityWindowHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Session.SendPacket($"wopen {packet.Type} 0");
        }
    }
}
