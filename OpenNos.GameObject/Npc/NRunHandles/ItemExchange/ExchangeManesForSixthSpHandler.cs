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
    [NRunHandler(NRunType.ExchangeForSP6)]
    public class ExchangeManesForSixthSpHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeManesForSixthSpHandler(ClientSession session) : base(session)
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
            if (packet.Type == 0)
            {
                Session.SendPacket($"qna #n_run^{packet.Runner}^56^{packet.Value}^{packet.NpcId} {Language.Instance.GetMessageFromKey("ASK_TRADE")}");
            }
            else
            {
                if (Session.Character.Inventory.CountItem(2523) < 50)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                    return;
                }
                switch (Session.Character.Class)
                {
                    case ClassType.Swordsman:
                        Session.Character.GiftAdd(4497, 1);
                        break;

                    case ClassType.Archer:
                        Session.Character.GiftAdd(4498, 1);
                        break;

                    case ClassType.Magician:
                        Session.Character.GiftAdd(4499, 1);
                        break;
                }
                Session.Character.Inventory.RemoveItemAmount(2523, 50);
            }
        }
    }
}
