using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.Handler.SharedMethods;
using NosByte.Shared;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class RemoveItemPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public RemoveItemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// remove packet
        /// </summary>
        /// <param name="removePacket"></param>
        public void Remove(RemovePacket removePacket)
        {
            if (removePacket != null)
            {
                InventoryType equipment;
                Mate mate = null;
                if (removePacket.Type > 0)
                {
                    equipment = (InventoryType)(12 + removePacket.Type);
                    mate = Session.Character.Mates.Find(s => s.MateType == MateType.Partner && s.PetId == removePacket.Type - 1);
                    if (mate.IsTemporalMate)
                    {
                        return;
                    }
                }
                else
                {
                    equipment = InventoryType.Wear;
                }

                if (Session.HasCurrentMapInstance
                    && Session.CurrentMapInstance.UserShops.FirstOrDefault(mapshop =>
                        mapshop.Value.OwnerId.Equals(Session.Character.CharacterId)).Value == null
                    && (Session.Character.ExchangeInfo == null
                     || (Session.Character.ExchangeInfo?.ExchangeList).Count == 0))
                {
                    ItemInstance inventory =
                        Session.Character.Inventory.LoadBySlotAndType(removePacket.InventorySlot, equipment);
                    if (inventory != null)
                    {
                        double currentRunningSeconds =
                            (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                        double timeSpanSinceLastSpUsage = currentRunningSeconds - Session.Character.LastSp;
                        if (removePacket.Type == 0)
                        {
                            if (removePacket.InventorySlot == (byte)EquipmentType.Sp && Session.Character.UseSp && !Session.Character.IsSeal)
                            {
                                if (Session.Character.IsVehicled)
                                {
                                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("REMOVE_VEHICLE"), 0));
                                    return;
                                }

                                if (Session.Character.LastSkillUse.AddSeconds(2) > DateTime.Now)
                                {
                                    return;
                                }

                                if (Session.Character.Timespace != null && Session.Character.Timespace.SpNeeded?[(byte)Session.Character.Class] != 0 && Session.Character.Timespace.InstanceBag.Lock)
                                {
                                    return;
                                }

                                Session.Character.SpRemoveConfirmation++;

                                if (Session.Character.SpRemoveConfirmation == 1)
                                {
                                    Observable.Timer(TimeSpan.FromSeconds(2)).SafeSubscribe(o =>
                                    {
                                        Session.Character.SpRemoveConfirmation = 0;
                                    });

                                    Session.SendPacket($"say 1 {Session.Character.CharacterId} 10 Press G again to remove your SP.");
                                    return;
                                }
                                else if (Session.Character.SpRemoveConfirmation == 2)
                                {
                                    Session.Character.RemoveSp(inventory.ItemVNum, false);
                                    Session.Character.LastSp = (DateTime.Now - Process.GetCurrentProcess().StartTime.AddSeconds(-50)).TotalSeconds;
                                }

                            }
                            else if (removePacket.InventorySlot == (byte)EquipmentType.Sp
                                     && !Session.Character.UseSp
                                     && timeSpanSinceLastSpUsage <= Session.Character.SpCooldown)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("SP_INLOADING"),
                                        Session.Character.SpCooldown - (int)Math.Round(timeSpanSinceLastSpUsage, 0)),
                                    0));
                                return;
                            }
                            else if (removePacket.InventorySlot == (byte)EquipmentType.Fairy
                                     && Session.Character.IsUsingFairyBooster)
                            {
                                Session.SendPacket($"qna n_run^31021^0 {Language.Instance.GetMessageFromKey("REMOVE_FAIRY_BOOSTER")}");
                                return;
                            }

                            if ((inventory.ItemDeleteTime >= DateTime.Now || inventory.DurabilityPoint > 0) && Session.Character.Buff.ContainsKey(62))
                            {
                                Session.Character.RemoveBuff(62);
                            }

                            Session.Character.EquipmentBCards.RemoveAll(o => o.ItemVNum == inventory.ItemVNum);
                        }

                        ItemInstance inv = Session.Character.Inventory.MoveInInventory(removePacket.InventorySlot,
                            equipment, InventoryType.Equipment);

                        if (inv == null)
                        {
                            Session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"),
                                    0));
                            return;
                        }

                        if (inv.Slot != -1)
                        {
                            Session.SendPacket(inventory.GenerateInventoryAdd());
                        }

                        if (removePacket.Type == 0)
                        {
                            if (Session.Character.UseSp)
                            {
                                Session.LoadSPStats();
                            }

                            Session.SendPackets(Session.Character.GenerateStatChar());
                            Session.SendPacket(Session.Character.GenerateEquipment());
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateEq());
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GeneratePairy());
                        }
                        else if (mate != null)
                        {
                            switch (inv.Item.EquipmentSlot)
                            {
                                case EquipmentType.Armor:
                                    mate.ArmorInstance = null;
                                    break;

                                case EquipmentType.MainWeapon:
                                    mate.WeaponInstance = null;
                                    break;

                                case EquipmentType.Gloves:
                                    mate.GlovesInstance = null;
                                    break;

                                case EquipmentType.Boots:
                                    mate.BootsInstance = null;
                                    break;

                                case EquipmentType.Sp:
                                    {
                                        if (mate.IsUsingSp)
                                        {
                                            mate.RemoveSp();
                                            mate.StartSpCooldown();
                                        }

                                        mate.Sp = null;
                                    }
                                    break;
                            }
                            mate.BattleEntity.BCards.RemoveAll(o => o.ItemVNum == inventory.ItemVNum);
                            Session.SendPacket(mate.GenerateScPacket());
                        }
                        ItemInstance ring = Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Ring, InventoryType.Wear);
                        ItemInstance bracelet = Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Bracelet, InventoryType.Wear);
                        ItemInstance necklace = Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Necklace, InventoryType.Wear);
                        Session.Character.CellonOptions.Clear();
                        if (ring != null)
                        {
                            Session.Character.CellonOptions.AddRange(ring.CellonOptions);
                        }
                        if (bracelet != null)
                        {
                            Session.Character.CellonOptions.AddRange(bracelet.CellonOptions);
                        }
                        if (necklace != null)
                        {
                            Session.Character.CellonOptions.AddRange(necklace.CellonOptions);
                        }
                        Session.SendPacket(Session.Character.GenerateStat());
                    }
                }
            }
        }
    }
}
