using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.Misc
{
    [NRunHandler(new [] { NRunType.FinishedTSDialog, NRunType.FinishedTSDialog2 })]
    public class FinishedTimeSpaceDialogHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public FinishedTimeSpaceDialogHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            
        }
    }
}
