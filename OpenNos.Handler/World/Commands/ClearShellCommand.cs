using System.Linq;
using NosByte.Packets.CommandPackets;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.Handler.World.Commands
{
    public class ClearShellCommand : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public ClearShellCommand(ClientSession session) => Session = session;

        public void Execute(ClearShellPacket packet)
        {
            var item = Session.Character.Inventory.LoadBySlotAndType(0, InventoryType.Equipment);

            if (item == null)
            {
                return;
            }

            if (packet.DeleteLastOption)
            {
                var lastOption = item.ShellEffects.Last();
                item.ShellEffects.Remove(lastOption);
                DAOFactory.ShellEffectDAO.DeleteOption(item.EquipmentSerialId, lastOption);
                Session.SendPacket(Session.Character.GenerateSay($"Last shell effect deleted.", 11));
                return;
            }
            item.ShellEffects.Clear();
            item.ShellRarity = null;
            DAOFactory.ShellEffectDAO.DeleteByEquipmentSerialId(item.EquipmentSerialId);
            Session.SendPacket(Session.Character.GenerateSay($"Shell effects deleted.", 11));
        }
    }

}
