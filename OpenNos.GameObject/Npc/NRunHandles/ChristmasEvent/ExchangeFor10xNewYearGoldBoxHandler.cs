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
    [NRunHandler(NRunType.ExchangeForHappyNewYearGoldenBox_x10)]
    public class ExchangeFor10xNewYearGoldBoxHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeFor10xNewYearGoldBoxHandler(ClientSession session) : base(session)
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
                if (Session.Character.Inventory.CountItem(1611) < 10 || Session.Character.Inventory.CountItem(1612) < 10
                                                                     || Session.Character.Inventory.CountItem(1613) < 20 || Session.Character.Inventory.CountItem(1614) < 10)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                    return;
                }
                Session.Character.GiftAdd(1621, 10);
                Session.Character.Inventory.RemoveItemAmount(1611, 10);
                Session.Character.Inventory.RemoveItemAmount(1612, 10);
                Session.Character.Inventory.RemoveItemAmount(1613, 20);
                Session.Character.Inventory.RemoveItemAmount(1614, 10);
            }
        }
    }
}
