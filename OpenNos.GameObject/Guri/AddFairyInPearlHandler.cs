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
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.AddFairyInPearl)]
    public class AddFairyInPearlHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public AddFairyInPearlHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (short.TryParse(packet.Value, out short fairySlot)
                && short.TryParse(packet.User.ToString(), out short pearlSlot))
            {
                ItemInstance fairy = Session.Character.Inventory.LoadBySlotAndType(fairySlot, InventoryType.Equipment);
                ItemInstance pearl = Session.Character.Inventory.LoadBySlotAndType(pearlSlot, InventoryType.Equipment);

                if (fairy?.Item == null || pearl?.Item == null)
                {
                    return;
                }

                if (!pearl.Item.IsHolder)
                {
                    return;
                }

                if (pearl.HoldingVNum > 0)
                {
                    return;
                }

                if (pearl.Item.ItemType == ItemType.Box && pearl.Item.ItemSubType == 5)
                {
                    if (fairy.Item.ItemType != ItemType.Jewelery || fairy.Item.ItemSubType != 3 || fairy.Item.IsDroppable)
                    {
                        return;
                    }

                    Session.Character.Inventory.RemoveItemFromInventory(fairy.Id);

                    pearl.HoldingVNum = fairy.ItemVNum;
                    pearl.ElementRate = fairy.ElementRate;

                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("FAIRY_SAVED"), 0));
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("FAIRY_SAVED"), 10));
                }
            }
        }
    }
}
