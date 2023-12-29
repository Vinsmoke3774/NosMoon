using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.Handler.World.Commands
{
    public class AddCellonOptionCommand : IPacketHandler
    {
        private ClientSession Session { get; }

        public AddCellonOptionCommand(ClientSession session) => Session = session;

        public void Execute(AddCellonPacket packet)
        {
            var item = Session.Character.Inventory.LoadBySlotAndType(packet.Slot, InventoryType.Equipment);

            if (item == null)
            {
                Session.SendPacket(Session.Character.GenerateSay($"No item found in slot {packet.Slot}", 11));
                return;
            }

            var option = new CellonOptionDTO
            {
                EquipmentSerialId = item.EquipmentSerialId,
                Level = packet.CellonLevel,
                Type = (CellonOptionType)packet.EffectType,
                Value = packet.Value
            };

            item.CellonOptions.Add(option);
            DAOFactory.CellonOptionDAO.InsertOrUpdate(option);
            Session.SendPacket(Session.Character.GenerateSay($"Cellon option added.", 11));
        }
    }
}
