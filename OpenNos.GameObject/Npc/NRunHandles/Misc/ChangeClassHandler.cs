using System.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Npc.NRunHandles.Misc
{
    [NRunHandler(NRunType.ChangeClass)]
    public class ChangeClassHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public ChangeClassHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (Session.Character.Class != (byte)ClassType.Adventurer)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ADVENTURER"), 0));
                return;
            }
            if (Session.Character.Level < 15 || Session.Character.JobLevel < 20)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_LVL"), 0));
                return;
            }
            if (packet.Type > 3 || packet.Type < 1)
            {
                return;
            }
            if (packet.Type == (byte)Session.Character.Class)
            {
                return;
            }

            if (Session.Character.Inventory.Values.All(s => s.Type == InventoryType.Wear))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("EQ_NOT_EMPTY"), 0));
                return;
            }

            Execute(packet);

        }

        public void Execute(NRunPacket packet)
        {

            switch (packet.Type)
            {
                case 1:
                    Session.Character.Inventory.AddNewToInventory(4964, 1, InventoryType.Wear, 7, 8, addStartingShell: true);
                    Session.Character.Inventory.AddNewToInventory(4961, 1, InventoryType.Wear, 7, 8, addStartingShell: true);
                    Session.Character.Inventory.AddNewToInventory(4952, 1, InventoryType.Wear, 7, 8, addStartingShell2: true);
                    Session.Character.Inventory.AddNewToInventory(4938, 1, InventoryType.Wear, addStartingShell3: true);
                    Session.Character.Inventory.AddNewToInventory(4936, 1, InventoryType.Wear, addStartingShell3: true);
                    Session.Character.Inventory.AddNewToInventory(4940, 1, InventoryType.Wear, addStartingShell3: true);
                    break;

                case 2:
                    Session.Character.Inventory.AddNewToInventory(4966, 1, InventoryType.Wear, 7, 8, addStartingShell: true);
                    Session.Character.Inventory.AddNewToInventory(4963, 1, InventoryType.Wear, 7, 8, addStartingShell: true);
                    Session.Character.Inventory.AddNewToInventory(4954, 1, InventoryType.Wear, 7, 8, addStartingShell2: true);
                    Session.Character.Inventory.AddNewToInventory(4938, 1, InventoryType.Wear, addStartingShell3: true);
                    Session.Character.Inventory.AddNewToInventory(4936, 1, InventoryType.Wear, addStartingShell3: true);
                    Session.Character.Inventory.AddNewToInventory(4940, 1, InventoryType.Wear, addStartingShell3: true);
                    break;

                case 3:
                    Session.Character.Inventory.AddNewToInventory(4965, 1, InventoryType.Wear, 7, 8, addStartingShell: true);
                    Session.Character.Inventory.AddNewToInventory(4962, 1, InventoryType.Wear, 7, 8, addStartingShell: true);
                    Session.Character.Inventory.AddNewToInventory(4953, 1, InventoryType.Wear, 7, 8, addStartingShell2: true);
                    Session.Character.Inventory.AddNewToInventory(4938, 1, InventoryType.Wear, addStartingShell3: true);
                    Session.Character.Inventory.AddNewToInventory(4936, 1, InventoryType.Wear, addStartingShell3: true);
                    Session.Character.Inventory.AddNewToInventory(4940, 1, InventoryType.Wear, addStartingShell3: true);
                    break;
            }

            Session.Character.Inventory.AddNewToInventory(380, 1, InventoryType.Equipment, upgrade: 6);
            Session.Character.Inventory.AddNewToInventory(381, 1, InventoryType.Equipment, upgrade: 6);
            Session.Character.Inventory.AddNewToInventory(382, 1, InventoryType.Equipment, upgrade: 6);
            Session.Character.Inventory.AddNewToInventory(383, 1, InventoryType.Equipment, upgrade: 6);
            Session.Character.Inventory.AddNewToInventory(388, 1, InventoryType.Equipment, upgrade: 6);
            Session.Character.Inventory.AddNewToInventory(389, 1, InventoryType.Equipment, upgrade: 6);
            Session.Character.Inventory.AddNewToInventory(390, 1, InventoryType.Equipment, upgrade: 6);
            Session.Character.Inventory.AddNewToInventory(391, 1, InventoryType.Equipment, upgrade: 6);
            Session.SendPacket(Session.Character.GenerateEquipment());
            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEq());
            Session.Character.ChangeClass((ClassType)packet.Type, false);
        }
    }
}
