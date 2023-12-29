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
    [GuriHandler(GuriType.AddMountInPearl)]
    public class AddMountInPearlHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public AddMountInPearlHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (short.TryParse(packet.Value, out short mountSlot)
                && short.TryParse(packet.User.ToString(), out short pearlSlot))
            {
                ItemInstance mount = Session.Character.Inventory.LoadBySlotAndType(mountSlot, InventoryType.Main);
                ItemInstance pearl = Session.Character.Inventory.LoadBySlotAndType(pearlSlot, InventoryType.Equipment);

                if (mount?.Item == null || pearl?.Item == null)
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

                if (pearl.Item.ItemType == ItemType.Box && pearl.Item.ItemSubType == 4)
                {
                    if (mount.Item.ItemType != ItemType.Special || mount.Item.ItemSubType != 0 || mount.Item.Speed < 1)
                    {
                        return;
                    }

                    Session.Character.Inventory.RemoveItemFromInventory(mount.Id);

                    pearl.HoldingVNum = mount.ItemVNum;

                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MOUNT_SAVED"), 0));
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MOUNT_SAVED"), 10));
                }
            }
        }
    }
}
