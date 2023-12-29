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
    [NRunHandler(NRunType.ExchangeForGlacerusRaidSeal)]
    public class ExchangeForGlacerusSealHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeForGlacerusSealHandler(ClientSession session) : base(session)
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
                if (Session.Character.Inventory.CountItem(1012) < 20 || Session.Character.Inventory.CountItem(2307) < 20 || Session.Character.Inventory.CountItem(5911) < 20)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                    return;
                }
                Session.Character.GiftAdd(5512, 1);
                Session.Character.Inventory.RemoveItemAmount(1012, 20);
                Session.Character.Inventory.RemoveItemAmount(2307, 20);
                Session.Character.Inventory.RemoveItemAmount(5911, 20);
            }
        }
    }
}
