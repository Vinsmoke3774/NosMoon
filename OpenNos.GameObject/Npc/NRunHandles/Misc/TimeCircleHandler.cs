using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.Misc
{
    [NRunHandler(NRunType.TimeCircle)]
    public class TimeCircleHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TimeCircleHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            Logger.Log.Warn($"Missing NRun handler: {NRunType.TimeCircle}");
        }
    }
}
