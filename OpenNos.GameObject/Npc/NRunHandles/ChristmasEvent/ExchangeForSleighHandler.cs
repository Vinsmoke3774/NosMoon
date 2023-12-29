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
    [NRunHandler(NRunType.ExchangeForChristmasRedNosedReindeerMagicSleigh)]
    public class ExchangeForSleighHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeForSleighHandler(ClientSession session) : base(session)
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
                if ((Session.Character.Inventory.CountItem(5712) < 1 && Session.Character.Inventory.CountItem(9138) < 1) || (Session.Character.Inventory.CountItem(4406) < 1 && Session.Character.Inventory.CountItem(8369) < 1))
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
                    return;
                }

                if (Session.Character.Inventory.CountItem(9138) > 0)
                {
                    Session.Character.Inventory.RemoveItemAmount(9138, 1);
                    Session.Character.GiftAdd(9140, 1);
                }
                else
                {
                    Session.Character.Inventory.RemoveItemAmount(5712, 1);
                    Session.Character.GiftAdd(5714, 1);
                }

                Session.Character.Inventory.RemoveItemAmount(Session.Character.Inventory.CountItem(8369) > 0 ? 8369 : 4406, 1);
            }
        }
    }
}
