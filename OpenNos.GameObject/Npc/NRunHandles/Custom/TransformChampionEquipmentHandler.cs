using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.TransformChampionEquipment)]
    public class TransformChampionEquipmentHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public TransformChampionEquipmentHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                return;
            }

            Execute(packet);
        }

        public void Execute(NRunPacket packet)
        {
            ItemInstance item = Session.Character.Inventory.LoadBySlotAndType(0, InventoryType.Equipment);

            if (item == null)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_HAVE_ITEM_IN_SLOT"), 0));
                return;
            }

            if (2000000000 > Session.Character.Gold)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 11));
                return;
            }

            ItemInstance newItem = item.DeepCopy();

            switch (item.Item.VNum)
            {
                // sword
                case 4949:
                case 4952:
                    newItem.ItemVNum = 4984;
                    break;

                case 4955:
                case 4961:
                    newItem.ItemVNum = 4978;
                    break;

                case 4958:
                case 4964:
                    newItem.ItemVNum = 4981;
                    break;

                // archer
                case 4951:
                case 4954:
                    newItem.ItemVNum = 4986;
                    break;

                case 4957:
                case 4963:
                    newItem.ItemVNum = 4980;
                    break;

                case 4960:
                case 4966:
                    newItem.ItemVNum = 4983;
                    break;

                // mage
                case 4950:
                case 4953:
                    newItem.ItemVNum = 4985;
                    break;

                case 4956:
                case 4962:
                    newItem.ItemVNum = 4979;
                    break;

                case 4959:
                case 4965:
                    newItem.ItemVNum = 4982;
                    break;

                default:
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_GOOD_STUFF"), 0));
                    return;
            }

            Session.Character.Gold -= 2000000000;
            Session.SendPacket(Session.Character.GenerateGold());
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("PAY_UPGRADE"), 12));
            newItem.EquipmentSerialId = item.EquipmentSerialId;
            newItem.Rare = 0;
            newItem.Upgrade = 0;
            newItem.SetRarityPoint();
            Session.Character.Inventory.TryRemove(item.Id, out _);
            Session.Character.Inventory.AddToInventory(newItem);
            Session.SendPacket(newItem.GenerateInventoryAdd());
            Session.SendPacket($"pdti 13 {newItem.ItemVNum} 0");
            Session.SendPacket("shop_end 1");
        }
    }
}
