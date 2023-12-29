using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.UserInterface
{
    [NRunHandler(NRunType.CellonItem)]
    public class CellonEnhancementHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public CellonEnhancementHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Session.SendPacket("wopen 3 0");
        }
    }
}
