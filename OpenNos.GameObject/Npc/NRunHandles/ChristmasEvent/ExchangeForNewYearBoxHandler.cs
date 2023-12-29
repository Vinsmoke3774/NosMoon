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
    [NRunHandler(NRunType.ExchangeForHappyNewYearBox)]
    public class ExchangeForNewYearBoxHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeForNewYearBoxHandler(ClientSession session) : base(session)
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
                if (Session.Character.Inventory.CountItem(1615) < 1 || Session.Character.Inventory.CountItem(1616) < 2 || Session.Character.Inventory.CountItem(1617) < 1
                    || Session.Character.Inventory.CountItem(1618) < 1 || Session.Character.Inventory.CountItem(1619) < 1 || Session.Character.Inventory.CountItem(1620) < 1)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                    return;
                }
                Session.Character.GiftAdd(1622, 1);
                Session.Character.Inventory.RemoveItemAmount(1615);
                Session.Character.Inventory.RemoveItemAmount(1616, 2);
                Session.Character.Inventory.RemoveItemAmount(1617);
                Session.Character.Inventory.RemoveItemAmount(1618);
                Session.Character.Inventory.RemoveItemAmount(1619);
                Session.Character.Inventory.RemoveItemAmount(1620);
            }
        }
    }
}
