using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Npc.NRunHandles.ItemExchange
{
    [NRunHandler(NRunType.ExchangePerfectionItem)]
    public class ExchangePerfectionItemHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangePerfectionItemHandler(ClientSession session) : base(session)
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
            switch (packet.Type)
            {
                case 0:
                    TradeItems(2522, 1, 2518, 1);
                    break;
                case 1:
                    TradeItems(2522, 1, 2514, 1);
                    break;
                case 2:
                    TradeItems(2522, 1, 2520, 1);
                    break;
                case 3:
                    TradeItems(2522, 1, 2516, 1);
                    break;
                case 4:
                    TradeItems(2523, 50, 2519, 1);
                    break;
                case 5:
                    TradeItems(2523, 50, 2515, 1);
                    break;
                case 6:
                    TradeItems(2523, 50, 2521, 1);
                    break;
                case 7:
                    TradeItems(2523, 50, 2517, 1);
                    break;
            }
        }
    }
}
