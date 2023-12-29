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

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.PerfumeItem)]
    public class PerfumeItemHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public PerfumeItemHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (packet.Argument == 0 && short.TryParse(packet.User.ToString(), out short slot))
            {
                const int perfumeVnum = 1428;

                InventoryType perfumeInventoryType = (InventoryType)packet.Argument;

                ItemInstance equipmentInstance = Session.Character.Inventory.LoadBySlotAndType(slot, perfumeInventoryType);

                if (equipmentInstance?.BoundCharacterId == null || equipmentInstance.BoundCharacterId == Session.Character.CharacterId || (equipmentInstance.Item.ItemType != ItemType.Weapon && equipmentInstance.Item.ItemType != ItemType.Armor))
                {
                    return;
                }

                int perfumesNeeded = ShellGeneratorHelper.Instance.PerfumeFromItemLevelAndShellRarity(equipmentInstance.Item.LevelMinimum, (byte)equipmentInstance.Rare, equipmentInstance.Item.IsHeroic);

                if (Session.Character.Inventory.CountItem(perfumeVnum) < perfumesNeeded)
                {
                    return;
                }

                Session.Character.Inventory.RemoveItemAmount(perfumeVnum, perfumesNeeded);

                equipmentInstance.BoundCharacterId = Session.Character.CharacterId;

                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BOUND_TO_YOU"), 0));
            }
        }
    }
}
