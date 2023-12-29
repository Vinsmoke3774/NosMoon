using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Npc.NRunHandles.ChristmasEvent
{
    [NRunHandler(NRunType.ExchangeFreshCakeForSnowmanSeal)]
    public class ExchangeFreshCakeForSnowmanSealHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeFreshCakeForSnowmanSealHandler(ClientSession session) : base(session)
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
            TradeItems(2327, 20, 1371, 1);
        }
    }
}
