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
    [NRunHandler(NRunType.GetQuestReturnBorrowedPowder)]
    public class ExchangeBorrowedPowerQuestHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ExchangeBorrowedPowerQuestHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                return;
            }

            if (!Session.Character.Quests.Any(s => s.Quest.QuestObjectives.Any(o => o.SpecialData == 5518)))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            short vNum = 0;
            for (short i = 4494; i <= 4496; i++)
            {
                if (Session.Character.Inventory.CountItem(i) > 0)
                {
                    vNum = i;
                    break;
                }
            }
            if (vNum > 0)
            {
                Session.Character.GiftAdd(5518, 1);
                Session.Character.GiftAdd(4504, 1);
                Session.Character.Inventory.RemoveItemAmount(vNum, 1);
            }
            else
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_INGREDIENTS"), 0));
            }
        }
    }
}
