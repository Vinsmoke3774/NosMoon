using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Extension.Item;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class UpgradePacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public UpgradePacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// up_gr packet
        /// </summary>
        /// <param name="upgradePacket"></param>
        public void Upgrade(UpgradePacket upgradePacket)
        {
            if (upgradePacket == null || Session.Character.ExchangeInfo?.ExchangeList.Count > 0
                || Session.Character.Speed == 0 || Session.Character.LastDelay.AddSeconds(5) > DateTime.Now)
            {
                return;
            }

            var inventoryType = upgradePacket.InventoryType;
            var uptype = upgradePacket.UpgradeType;
            var slot = upgradePacket.Slot;
            var slot2 = upgradePacket.Slot2;
            Session.Character.LastDelay = DateTime.Now;
            ItemInstance inventory;
            switch (uptype)
            {
                // Fusion Scroll
                case 53:
                    {
                        inventory = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                        var secondItem = Session.Character.Inventory.LoadBySlotAndType((byte)slot2, inventoryType);
                        if (inventory == null)
                        {
                            return;
                        }

                        if (secondItem == null)
                        {
                            return;
                        }

                        inventory.FusionItem(Session, secondItem);
                    }
                    break;

                // Craft tattoo
                case 81:
                    {
                        inventory = Session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Main);

                        if (inventory == null)
                        {
                            return;
                        }

                        inventory.CraftTattoo(Session);
                    }
                    break;

                // Tattoo Upgrade / Remove
                case 82:
                case 85:
                    {
                        var ski = Session.Character.Skills.FirstOrDefault(s => s.SkillVNum == slot);

                        if (ski == null) return;

                        switch (inventoryType)
                        {
                            // Remove Tattoo Inked
                            case (InventoryType)2:
                                if (uptype != 82) return;
                                ski.RemoveTattoo(Session);
                                break;

                            // Upgrade Tattoo
                            case (InventoryType)1: // NPC
                            case (InventoryType)3: // with scroll
                                ski.UpgradeTattoo(Session, uptype != 82);
                                break;

                            default:
                                return;
                        }
                    }
                    break;

                // Rune Upgrade / Remove
                case 83:
                case 84: // scroll premium
                case 86: // scroll basic
                    {
                        inventory = Session.Character.Inventory.LoadBySlotAndType((byte)upgradePacket.InventoryType2,
                            InventoryType.Equipment);

                        if (inventory == null)
                        {
                            return;
                        }

                        switch (inventoryType)
                        {
                            // Remove Rune
                            case (InventoryType)2:
                                inventory.RemoveRune(Session);
                                break;

                            // Upgrade Rune
                            case (InventoryType)1:
                            case (InventoryType)3: // basic
                            case (InventoryType)4: // Premium
                                inventory.UpgradeRune(Session,
                                    uptype == 84 ? UpgradeRuneType.Premium :
                                    uptype == 86 ? UpgradeRuneType.Basic : UpgradeRuneType.None);
                                break;

                            default:
                                return;
                        }
                    }
                    break;

                case 0:
                    inventory = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (inventory != null)
                    {
                        if ((inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                        {
                            inventory.ConvertToPartnerEquipment(Session);
                        }
                    }
                    break;

                case 1:
                    inventory = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (inventory != null)
                    {
                        if ((inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                        {
                            inventory.UpgradeItem(Session, UpgradeMode.Normal, UpgradeProtection.None);
                        }
                    }
                    break;

                case 3:

                    //up_gr 3 0 0 7 1 1 20 99
                    string[] originalSplit = upgradePacket.OriginalContent.Split(' ');
                    if (originalSplit.Length == 10
                        && byte.TryParse(originalSplit[5], out byte firstSlot)
                        && byte.TryParse(originalSplit[8], out byte secondSlot))
                    {
                        inventory = Session.Character.Inventory.LoadBySlotAndType(firstSlot, InventoryType.Equipment);
                        if (inventory != null
                            && (inventory.Item.EquipmentSlot == EquipmentType.Necklace
                             || inventory.Item.EquipmentSlot == EquipmentType.Bracelet
                             || inventory.Item.EquipmentSlot == EquipmentType.Ring)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                        {
                            ItemInstance cellon =
                                Session.Character.Inventory.LoadBySlotAndType(secondSlot,
                                    InventoryType.Main);
                            if (cellon?.ItemVNum > 1016 && cellon.ItemVNum < 1027)
                            {
                                inventory.OptionItem(Session, cellon.ItemVNum);
                            }
                        }
                    }
                    break;

                case 7:
                case 21:
                    {
                        inventory = Session.Character.Inventory.LoadBySlotAndType(slot, InventoryType.Equipment);
                        if (inventory == null)
                        {
                            return;
                        }
                        if ((inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                        {
                            RarifyMode mode = RarifyMode.Normal;
                            RarifyProtection protection = RarifyProtection.None;
                            ItemInstance amulet = Session.Character.Inventory.LoadBySlotAndType((short)EquipmentType.Amulet, InventoryType.Wear);
                            if (amulet != null)
                            {
                                switch (amulet.Item.Effect)
                                {
                                    case 791:
                                        protection = RarifyProtection.RedAmulet;
                                        break;

                                    case 792:
                                        protection = RarifyProtection.BlueAmulet;
                                        break;

                                    case 794:
                                        protection = RarifyProtection.HeroicAmulet;
                                        break;

                                    case 795:
                                        protection = RarifyProtection.RandomHeroicAmulet;
                                        break;

                                    case 796:
                                        if (inventory.Item.IsHeroic)
                                        {
                                            mode = RarifyMode.Success;
                                        }
                                        break;

                                    case 797:
                                        protection = RarifyProtection.RandomOlorunAmulet;
                                        break;

                                    case 798:
                                        protection = RarifyProtection.OlorunAmulet;
                                        break;

                                    case 793:
                                        ItemInstance equip = Session.Character.Inventory.LoadBySlotAndType(slot, (InventoryType.Equipment));
                                        if (equip?.IsFixed == true)
                                        {
                                            Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 3003));
                                            Session.SendPacket(UserInterfaceHelper.GenerateGuri(17, 1, Session.Character.CharacterId, slot));
                                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_UNFIXED"), 11));
                                        }
                                        break;
                                }
                            }

                            if (uptype == 7)
                            {
                                if (inventoryType == InventoryType.Equipment && protection == RarifyProtection.None && inventory.Item.IsHeroic)
                                {
                                    Session.SendPacket($"qna #up_gr^7^1^{slot} {Language.Instance.GetMessageFromKey("REALLY_WANT_GAMBLE")}");
                                    return;
                                }
                                inventory.RarifyItem(Session, mode, protection);
                                Session.SendPacket("shop_end 1");
                                return;
                            }

                            inventory.RarifyItem(Session, RarifyMode.Normal, RarifyProtection.Scroll);
                        }
                    }
                    break;

                case 8:
                    inventory = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (upgradePacket.InventoryType2 != null && upgradePacket.Slot2 != null)
                    {
                        ItemInstance inventory2 =
                            Session.Character.Inventory.LoadBySlotAndType((byte)upgradePacket.Slot2,
                                (InventoryType)upgradePacket.InventoryType2);

                        if (inventory != null && inventory2 != null && !Equals(inventory, inventory2))
                        {
                            inventory.Sum(Session, inventory2);
                        }
                    }
                    break;

                case 9:
                    ItemInstance specialist = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.UpgradeSp(Session, UpgradeProtection.None);
                            }
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 20:
                    inventory = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (inventory != null)
                    {
                        if ((inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                        {
                            inventory.UpgradeItem(Session, UpgradeMode.Normal, UpgradeProtection.Protected);
                        }
                    }
                    break;

                case 25:
                    specialist = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Upgrade > 9)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("MUST_USE_ITEM"), ServerManager.GetItem(1364).Name), 0));
                                return;
                            }
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.UpgradeSp(Session, UpgradeProtection.Protected);
                            }
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 26:
                    specialist = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Upgrade <= 9)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("MUST_USE_ITEM"), ServerManager.GetItem(1363).Name), 0));
                                return;
                            }
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.UpgradeSp(Session, UpgradeProtection.Protected);
                            }
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 38:
                case 35:
                case 42:
                    specialist = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.UpgradeSp(Session, UpgradeProtection.Event);
                            }
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 41:
                    specialist = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (specialist != null)
                    {
                        if (specialist.Rare != -2)
                        {
                            if (specialist.Item.EquipmentSlot == EquipmentType.Sp)
                            {
                                specialist.PerfectSp(Session);
                            }
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                Language.Instance.GetMessageFromKey("CANT_UPGRADE_DESTROYED_SP"), 0));
                        }
                    }
                    break;

                case 43:
                    inventory = Session.Character.Inventory.LoadBySlotAndType(slot, inventoryType);
                    if (inventory != null)
                    {
                        if ((inventory.Item.EquipmentSlot == EquipmentType.Armor
                             || inventory.Item.EquipmentSlot == EquipmentType.MainWeapon
                             || inventory.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
                            && inventory.Item.ItemType != ItemType.Shell && inventory.Item.Type == InventoryType.Equipment)
                        {
                            inventory.UpgradeItem(Session, UpgradeMode.Reduced, UpgradeProtection.Protected);
                        }
                    }
                    break;
            }
        }
    }
}
