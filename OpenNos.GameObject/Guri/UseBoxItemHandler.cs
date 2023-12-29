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
    [GuriHandler(GuriType.UseBoxItem)]
    public class UseBoxItemHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public UseBoxItemHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (packet.Argument == 8023 && short.TryParse(packet.User.ToString(), out short slot))
            {
                ItemInstance box = Session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Equipment);
                box?.Item.Use(Session, ref box, 1, new[] { packet.Data.ToString() });
            }
        }
    }
}
