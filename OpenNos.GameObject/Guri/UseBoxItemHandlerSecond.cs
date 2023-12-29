using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.UseBoxItem2)]
    public class UseBoxItemSecondHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public UseBoxItemSecondHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            short.TryParse(packet.User.ToString(), out short slot);
            ItemInstance inv = Session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Equipment);

            if (packet.Argument == inv.ItemVNum && packet.User == slot)
            {
                inv = Session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Equipment);
                inv.ItemDeleteTime = DateTime.Now.AddHours(inv.Item.LevelMinimum);
                inv.SpStoneUpgrade = 1;
                inv.BoundCharacterId = Session.Character.CharacterId;
                Session.SendPacket($"ivn 0 {inv.Slot}.{inv.ItemVNum}.{inv.Rare}.{(inv.Item.IsColored ? inv.Design : inv.Upgrade)}.{inv.SpStoneUpgrade}.{inv.RuneAmount}");
            }
        }
    }
}
