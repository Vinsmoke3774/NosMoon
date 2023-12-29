using NosByte.Packets.ClientPackets;
using NosByte.Shared;
using OpenNos.Core.Actions;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Helpers;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject.Npc.NRunHandles.Custom
{
    [NRunHandler(NRunType.AutoBet)]
    public class AutoBetHandler : SpecialHandlerBase, IGenericHandler<NRunPacket>
    {
        public AutoBetHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(NRunPacket packet)
        {
            if (!InitializeNpc(packet))
            {
                return;
            }

            if (Session.Character.AutoBetInterval != null)
            {
                return;
            }

            Execute(packet);
        }

        private RarifyProtection GetRarifyProtection(ItemInstance amulet)
        {
            if (amulet == null)
            {
                return RarifyProtection.None;
            }

            switch (amulet.Item.Effect)
            {
                case 791:
                    return RarifyProtection.RedAmulet;

                case 792:
                    return RarifyProtection.BlueAmulet;

                case 794:
                    return RarifyProtection.HeroicAmulet;

                case 795:
                    return RarifyProtection.RandomHeroicAmulet;

                case 797:
                    return RarifyProtection.RandomOlorunAmulet;

                case 798:
                    return RarifyProtection.OlorunAmulet;
            }

            return RarifyProtection.None;
        }

        public void Execute(NRunPacket packet)
        {
            try
            {
                int i = 0;

                Session.Character.AutoBetInterval = Observable.Interval(TimeSpan.FromMilliseconds(100)).SafeSubscribe(x =>
                {
                    try
                    {
                        if (Session?.Character?.Inventory == null)
                        {
                            return;
                        }

                        if (Session.Character.Gold < 500)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("You need at least 500 gold to bet."));
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        if (Session.Character.Inventory.CountItem(1014) < 5)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("You need at least 5 cella powder to bet."));
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        var equipment = Session.Character.Inventory.LoadBySlotAndType(0, InventoryType.Equipment);

                        if (equipment == null)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("You need to put your equipment in your first inventory slot."));
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        if (equipment.Item.ItemType != ItemType.Armor && equipment.Item.ItemType != ItemType.Weapon)
                        {
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        var allowedAmulets = new[] { 791, 792, 794, 795, 797, 798 };

                        var amulets = Session.Character.Inventory.Values.Where(s => allowedAmulets.Any(x => x == s.Item.Effect) && (s.Type == InventoryType.Equipment || s.Type == InventoryType.Wear) && s.Item.Type == InventoryType.Equipment).ToList();

                        if (!amulets.Any())
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("You need at least 1 amulet to auto-bet."));
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        if (amulets.Select(s => s.ItemVNum).ToHashSet().Count > 1)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("You must have only one type of amulets in your inventory to auto-bet."));
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        var alreadyEquipped = Session.Character.Inventory.LoadBySlotAndType((short)EquipmentType.Amulet, InventoryType.Wear);

                        if (alreadyEquipped != null && allowedAmulets.All(s => s != alreadyEquipped.Item.Effect))
                        {
                            Session.ReceivePacket($"remove 11 0"); //Remove item that might be in amulet slot already
                        }

                        i++;

                        if (i > 5000)
                        {
                            Logger.Log.Error($"Infinite loop spotted while auto betting.", null);
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            Session.Disconnect();
                            return;
                        }

                        if (!amulets.Any())
                        {
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        ItemInstance amulet = Session.Character?.Inventory?.LoadBySlotAndType((short)EquipmentType.Amulet, InventoryType.Wear);

                        if (amulet == null)
                        {
                            var availableAmulet = amulets.FirstOrDefault();

                            if (availableAmulet == null)
                            {
                                Session.Character.AutoBetInterval?.Dispose();
                                Session.Character.AutoBetInterval = null;
                                return;
                            }
                            Session.ReceivePacket($"wear {availableAmulet.Slot} 0"); // equip amulet<
                        }

                        amulet = Session.Character.Inventory.LoadBySlotAndType((short)EquipmentType.Amulet, InventoryType.Wear);
                        var protection = GetRarifyProtection(amulet);

                        if ((protection == RarifyProtection.BlueAmulet || protection == RarifyProtection.RedAmulet) &&
                            equipment.Item.IsHeroic)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("You need to use champion amulets for champion equipments"));
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        if (protection == RarifyProtection.None)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("An error occurred while trying to bet."));
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        if (equipment.Rare == 8)
                        {
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }

                        equipment.RarifyItem(Session, RarifyMode.Reduced, protection, isAutoBet: true);

                        if (equipment.Rare >= 7)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("Your item reached rarity 7 or higher."));
                            Session.Character.AutoBetInterval?.Dispose();
                            Session.Character.AutoBetInterval = null;
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        if (Session?.Character == null)
                        {
                            return;
                        }

                        Session.Character.AutoBetInterval?.Dispose();
                        Session.Character.AutoBetInterval = null;
                        Logger.Log.Error(null, e);
                    }
                });
            }
            catch (Exception e)
            {
                Session.Character.AutoBetInterval?.Dispose();
                Session.Character.AutoBetInterval = null;
                Logger.Log.Error(null, e);
            }
        }
    }
}
