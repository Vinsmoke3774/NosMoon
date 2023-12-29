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
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Npc.NRunHandles.ChristmasEvent
{
    [NRunHandler(NRunType.ExchangeForHappyNewYearBox_x10)]
    public class ExchangeFor10xNewYearBoxHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeFor10xNewYearBoxHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet) || !ServerManager.Instance.Configuration.ChristmasEvent)
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
                if (Session.Character.Inventory.CountItem(1615) < 10 || Session.Character.Inventory.CountItem(1616) < 20 || Session.Character.Inventory.CountItem(1617) < 10
                    || Session.Character.Inventory.CountItem(1618) < 10 || Session.Character.Inventory.CountItem(1619) < 10 || Session.Character.Inventory.CountItem(1620) < 10)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                    return;
                }
                Session.Character.GiftAdd(1622, 10);
                Session.Character.Inventory.RemoveItemAmount(1615, 10);
                Session.Character.Inventory.RemoveItemAmount(1616, 20);
                Session.Character.Inventory.RemoveItemAmount(1617, 10);
                Session.Character.Inventory.RemoveItemAmount(1618, 10);
                Session.Character.Inventory.RemoveItemAmount(1619, 10);
                Session.Character.Inventory.RemoveItemAmount(1620, 10);
            }
        }
    }
}
