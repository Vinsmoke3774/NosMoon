/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace OpenNos.GameObject
{
    public class MagicalItem : Item
    {
        #region Instantiation

        public MagicalItem(ItemDTO item) : base(item)
        {
        }

        #endregion

        #region Methods

        public override void Use(ClientSession session, ref ItemInstance inv, byte Option = 0, string[] packetsplit = null)
        {
            if (session.Character.IsVehicled)
            {
                session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_DO_VEHICLED"), 10));
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.ArenaInstance)
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RainbowBattleInstance)
            {
                return;
            }

            switch (Effect)
            {
                // airwaves - eventitems
                case 0:
                    if (inv.Item.ItemType == ItemType.Shell)
                    {
                        if (!(inv).ShellEffects.Any())
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHELL_MUST_BE_IDENTIFIED"), 0));
                            return;
                        }

                        if (packetsplit?.Length < 9)
                        {
                            return;
                        }

                        if (!int.TryParse(packetsplit[6], out int requestType))
                        {
                            return;
                        }
                        if (!Enum.TryParse(packetsplit[8], out InventoryType eqType))
                        {
                            return;
                        }

                        if (inv.ShellEffects.Count != 0 && packetsplit?.Length > 9 && byte.TryParse(packetsplit[9], out byte islot))
                        {
                            ItemInstance wearable = session.Character.Inventory.LoadBySlotAndType(islot, InventoryType.Equipment);

                            var heroicShell = inv.Item.IsHeroic;

                            if (heroicShell && !wearable.Item.IsHeroic)
                            {
                                return;
                            }

                            if (wearable == null)
                            {
                                return;
                            }

                            var isLevelAccepted = wearable.Item.LevelMinimum >= (heroicShell ? 0 : inv.Upgrade);

                            if (wearable.Item.ItemType != ItemType.Weapon && wearable.Item.ItemType != ItemType.Armor)
                            {
                                return;
                            }

                            if (!isLevelAccepted)
                            {
                                return;
                            }

                            if (wearable.Rare < inv.Rare)
                            {
                                return;
                            }
                            switch (requestType)
                            {
                                case 0:
                                    session.SendPacket($"qna #u_i^1^{session.Character.CharacterId}^{(short)inv.Type}^{inv.Slot}^1^1^{(short)eqType}^{islot} {Language.Instance.GetMessageFromKey("ADD_OPTION_ON_STUFF")}");
                                    break;

                                case 1:

                                    if (inv.ShellEffects == null)
                                    {
                                        return;
                                    }
                                    if (wearable.BoundCharacterId != session.Character.CharacterId && wearable.BoundCharacterId != null)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NEED_FRAGANCE"), 0));
                                        return;
                                    }
                                    if (wearable.Rare < inv.Rare)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHELL_RARITY_TOO_HIGH"), 0));
                                        return;
                                    }

                                    bool weapon = inv.Item.ItemSubType == 0;
                                    if ((wearable.Item.ItemType == ItemType.Weapon && weapon) || (wearable.Item.ItemType == ItemType.Armor && !weapon))
                                    {
                                        wearable.BoundCharacterId = session.Character.CharacterId;
                                        wearable.ShellRarity = inv.Rare;
                                        wearable.ShellEffects.Clear();
                                        DAOFactory.ShellEffectDAO.DeleteByEquipmentSerialId(wearable.EquipmentSerialId);
                                        wearable.ShellEffects.AddRange(inv.ShellEffects);
                                        if (wearable.EquipmentSerialId == Guid.Empty)
                                        {
                                            wearable.EquipmentSerialId = Guid.NewGuid();
                                        }
                                        DAOFactory.ShellEffectDAO.InsertOrUpdateFromList(wearable.ShellEffects, wearable.EquipmentSerialId);
                                        session.Character.DeleteItemByItemInstanceId(inv.Id);
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("OPTION_SUCCESS"), 0));
                                    }
                                    break;
                            }
                        }
                        return;
                    }

                    if (ItemType == ItemType.Magical)
                    {
                        switch (VNum)
                        {
                            // Bank Card
                            case 2539:
                            case 10066:
                                session.OpenBank();
                                return;
                        }
                    }
                    if (ItemType == ItemType.Event)
                    {
                        if (ItemHelper.PassiveEventItems.Any(s => s.Equals(VNum)))
                        {
                            return;
                        }

                        session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, EffectValue));
                        if (MappingHelper.GuriItemEffects.ContainsKey(EffectValue))
                        {
                            session.CurrentMapInstance?.Broadcast(UserInterfaceHelper.GenerateGuri(19, 1, session.Character.CharacterId, MappingHelper.GuriItemEffects[EffectValue]), session.Character.PositionX, session.Character.PositionY);
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                //respawn objects
                case 1:
                    if (session.Character.MapInstance.MapInstanceType != MapInstanceType.BaseMapInstance || session.Character.IsSeal)
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_USE_THAT"), 10));
                        return;
                    }
                    int type, secondaryType, inventoryType, slot;
                    if (packetsplit?.Length > 6 && int.TryParse(packetsplit[2], out type) && int.TryParse(packetsplit[3], out secondaryType) && int.TryParse(packetsplit[4], out inventoryType) && int.TryParse(packetsplit[5], out slot))
                    {
                        int packetType;
                        switch (EffectValue)
                        {
                            case 0:
                                if (inv.ItemVNum != 2070 && inv.ItemVNum != 10010 && inv.ItemVNum != 2611)
                                {
                                    return;
                                }
                                if (Option == 0)
                                {
                                    if (ServerManager.Instance.ChannelId == 51)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateDialog($"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^2 #u_i^{type}^{secondaryType}^{inventoryType}^{slot}^0 {Language.Instance.GetMessageFromKey("WANT_TO_GO_BASE")}"));
                                    }
                                    else
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateDialog($"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1 #u_i^{type}^{secondaryType}^{inventoryType}^{slot}^2 {Language.Instance.GetMessageFromKey("WANT_TO_SAVE_POSITION")}"));
                                    }
                                }
                                else if (int.TryParse(packetsplit[6], out packetType))
                                {
                                    switch (packetType)
                                    {
                                        case 1:
                                            session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^3"));
                                            break;

                                        case 2:
                                            session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^4"));
                                            break;

                                        case 3:
                                            session.Character.SetReturnPoint(session.Character.MapId, session.Character.MapX, session.Character.MapY);
                                            RespawnMapTypeDTO respawn = session.Character.Respawn;
                                            if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                            {
                                                MapCell mapCell = new MapCell();
                                                for (int i = 0; i < 5; i++)
                                                {
                                                    mapCell.X = (short)(respawn.DefaultX + ServerManager.RandomNumber(-3, 3));
                                                    mapCell.Y = (short)(respawn.DefaultY + ServerManager.RandomNumber(-3, 3));
                                                    if (ServerManager.GetMapInstanceByMapId(respawn.DefaultMapId) is MapInstance GoToMap)
                                                    {
                                                        if (!GoToMap.Map.IsBlockedZone(mapCell.X, mapCell.Y))
                                                        {
                                                            break;
                                                        }
                                                    }
                                                }
                                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawn.DefaultMapId, mapCell.X, mapCell.Y);
                                            }
                                            if (inv.ItemVNum != 2611)
                                            {
                                                session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                            }
                                            break;

                                        case 4:
                                            RespawnMapTypeDTO respawnObj = session.Character.Respawn;
                                            if (ServerManager.Instance.ChannelId == 51)
                                            {
                                                respawnObj.DefaultMapId = (short)(129 + session.Character.Faction);
                                                respawnObj.DefaultX = 41;
                                                respawnObj.DefaultY = 42;
                                            }
                                            if (respawnObj.DefaultX != 0 && respawnObj.DefaultY != 0 && respawnObj.DefaultMapId != 0)
                                            {
                                                MapCell mapCell = new MapCell();
                                                for (int i = 0; i < 5; i++)
                                                {
                                                    mapCell.X = (short)(respawnObj.DefaultX + ServerManager.RandomNumber(-3, 3));
                                                    mapCell.Y = (short)(respawnObj.DefaultY + ServerManager.RandomNumber(-3, 3));
                                                    if (ServerManager.GetMapInstanceByMapId(respawnObj.DefaultMapId) is MapInstance GoToMap)
                                                    {
                                                        if (!GoToMap.Map.IsBlockedZone(mapCell.X, mapCell.Y))
                                                        {
                                                            break;
                                                        }
                                                    }
                                                }
                                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawnObj.DefaultMapId, mapCell.X, mapCell.Y);
                                            }
                                            if (inv.ItemVNum != 2611)
                                            {
                                                session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                            }
                                            break;
                                    }
                                }
                                break;

                            case 1:
                                if (inv.ItemVNum != 2071 && inv.ItemVNum != 10011)
                                {
                                    return;
                                }
                                if (int.TryParse(packetsplit[6], out packetType))
                                {
                                    if (ServerManager.Instance.ChannelId == 51)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANNOT_USE"), 10));
                                        return;
                                    }
                                    RespawnMapTypeDTO respawn = session.Character.Return;
                                    switch (packetType)
                                    {
                                        case 0:
                                            if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                            {
                                                session.SendPacket(UserInterfaceHelper.GenerateRp(respawn.DefaultMapId, respawn.DefaultX, respawn.DefaultY, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1"));
                                            }
                                            break;

                                        case 1:
                                            session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^2"));
                                            break;

                                        case 2:
                                            if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                            {
                                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawn.DefaultMapId, respawn.DefaultX, respawn.DefaultY);
                                            }
                                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);

                                            break;
                                    }
                                }
                                break;

                            case 2:
                                if (inv.ItemVNum != 2072 && inv.ItemVNum != 10012)
                                {
                                    return;
                                }
                                if (Option == 0)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1"));
                                }
                                else
                                {
                                    ServerManager.Instance.JoinMiniland(session, session);
                                    session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                }
                                break;

                            case 4:
                                if (inv.ItemVNum != 2188 || session.Character.MapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act4))
                                {
                                    return;
                                }
                                if (Option == 0)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1"));
                                }
                                else
                                {
                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, 98, 28, 34);
                                    session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                }
                                break;

                            case 5:
                                if (inv.ItemVNum != 2311 || ServerManager.Instance.ChannelId != 51)
                                {
                                    return;
                                }
                                if (ServerManager.GetAllMapInstances().SelectMany(s => s.Monsters.ToList()).LastOrDefault(s => s.MonsterVNum == (short)session.Character.Faction + 964) is MapMonster flag)
                                {
                                    if (Option == 0)
                                    {
                                        session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1"));
                                    }
                                    else
                                    {
                                        ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId, flag.MapInstance.MapInstanceId, flag.MapX, flag.MapY);
                                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                    }
                                }
                                break;

                            case 6:
                                if (inv.ItemVNum != 2384 || ServerManager.Instance.ChannelId == 51 || session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
                                {
                                    return;
                                }
                                if (Option == 0)
                                {
                                    session.SendPacket(UserInterfaceHelper.GenerateDialog($"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^1 #u_i^{type}^{secondaryType}^{inventoryType}^{slot}^2 {Language.Instance.GetMessageFromKey("WANT_TO_SAVE_POSITION")}"));
                                }
                                else if (int.TryParse(packetsplit[6], out packetType))
                                {
                                    switch (packetType)
                                    {
                                        case 1:
                                            session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^3"));
                                            break;

                                        case 2:
                                            session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 7, $"#u_i^{type}^{secondaryType}^{inventoryType}^{slot}^4"));
                                            break;

                                        case 3:
                                            if (session.CurrentMapInstance.Map.MapTypes.Any(s => s.MapTypeId == (short)MapTypeEnum.Act51 || s.MapTypeId == (short)MapTypeEnum.Act52))
                                            {
                                                session.Character.SetReturnPoint(session.Character.MapId, session.Character.MapX, session.Character.MapY);
                                                RespawnMapTypeDTO respawn = session.Character.Respawn;
                                                if (respawn.DefaultX != 0 && respawn.DefaultY != 0 && respawn.DefaultMapId != 0)
                                                {
                                                    MapCell mapCell = new MapCell();
                                                    for (int i = 0; i < 5; i++)
                                                    {
                                                        mapCell.X = (short)(respawn.DefaultX + ServerManager.RandomNumber(-3, 3));
                                                        mapCell.Y = (short)(respawn.DefaultY + ServerManager.RandomNumber(-3, 3));
                                                        if (ServerManager.GetMapInstanceByMapId(respawn.DefaultMapId) is MapInstance GoToMap)
                                                        {
                                                            if (!GoToMap.Map.IsBlockedZone(mapCell.X, mapCell.Y))
                                                            {
                                                                break;
                                                            }
                                                        }
                                                    }
                                                    ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawn.DefaultMapId, mapCell.X, mapCell.Y);
                                                }
                                                session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                            }
                                            else
                                            {
                                                goto case 4;
                                            }
                                            break;

                                        case 4:
                                            MapInstance mapInstanceBackup = session.CurrentMapInstance;
                                            session.CurrentMapInstance = ServerManager.GetMapInstanceByMapId(170);
                                            RespawnMapTypeDTO respawnObj = session.Character.Respawn;
                                            session.CurrentMapInstance = mapInstanceBackup;
                                            if (ServerManager.Instance.ChannelId == 51)
                                            {
                                                respawnObj.DefaultMapId = (short)(129 + session.Character.Faction);
                                                respawnObj.DefaultX = 41;
                                                respawnObj.DefaultY = 42;
                                            }
                                            if (respawnObj.DefaultX != 0 && respawnObj.DefaultY != 0 && respawnObj.DefaultMapId != 0)
                                            {
                                                MapCell mapCell = new MapCell();
                                                for (int i = 0; i < 5; i++)
                                                {
                                                    mapCell.X = (short)(respawnObj.DefaultX + ServerManager.RandomNumber(-3, 3));
                                                    mapCell.Y = (short)(respawnObj.DefaultY + ServerManager.RandomNumber(-3, 3));
                                                    if (ServerManager.GetMapInstanceByMapId(respawnObj.DefaultMapId) is MapInstance GoToMap)
                                                    {
                                                        if (!GoToMap.Map.IsBlockedZone(mapCell.X, mapCell.Y))
                                                        {
                                                            break;
                                                        }
                                                    }
                                                }
                                                ServerManager.Instance.ChangeMap(session.Character.CharacterId, respawnObj.DefaultMapId, mapCell.X, mapCell.Y);
                                            }
                                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                    break;

                // Dowsing Rod (To find energy field)
                case 4:
                    session.SendPacket(UserInterfaceHelper.GenerateDelay(2000, 2, $"#guri^700"));
                    break;

                // dyes or waxes
                case 10:
                case 11:
                    if (!session.Character.IsVehicled)
                    {
                        if (Effect == 10)
                        {
                            if (EffectValue == 99)
                            {
                                byte nextValue = (byte)ServerManager.RandomNumber(0, 127);
                                session.Character.HairColor = Enum.IsDefined(typeof(HairColorType), nextValue) ? (HairColorType)nextValue : 0;
                            }
                            else
                            {
                                session.Character.HairColor = Enum.IsDefined(typeof(HairColorType), (byte)EffectValue) ? (HairColorType)EffectValue : 0;
                            }
                        }
                        else if (Effect == 11)
                        {
                            if (session.Character.Class == (byte)ClassType.Adventurer && EffectValue > 1)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
                                return;
                            }
                            if (session.Character.Gender != (GenderType)Sex && (VNum < 2130 || VNum > 2162) && (VNum < 10025 || VNum > 10026))
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANNOT_USE"), 10));
                                return;
                            }
                            session.Character.HairStyle = Enum.IsDefined(typeof(HairStyleType), (byte)EffectValue) ? (HairStyleType)EffectValue : 0;
                        }
                        else
                        {
                            if (session.Character.Class == (byte)ClassType.Adventurer && EffectValue > 1)
                            {
                                session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ADVENTURERS_CANT_USE"), 10));
                                return;
                            }
                            session.Character.HairStyle = Enum.IsDefined(typeof(HairStyleType), (byte)EffectValue) ? (HairStyleType)EffectValue : 0;
                        }
                        session.SendPacket(session.Character.GenerateEq());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                // TS npcs health
                case 12:
                    if (EffectValue > 0)
                    {
                        if (session.Character.Timespace != null)
                        {
                            session.Character.MapInstance.GetBattleEntitiesInRange(new MapCell { X = session.Character.PositionX, Y = session.Character.PositionY }, 6)
                                .Where(s => (s.MapNpc != null || (s.Mate != null && s.Mate.IsTemporalMate)) && !session.Character.BattleEntity.CanAttackEntity(s)).ToList()
                                .ForEach(s =>
                                {
                                    int health = EffectValue;
                                    if (s.Hp + EffectValue > s.HpMax)
                                    {
                                        health = s.HpMax - s.Hp;
                                    }
                                    s.Hp += health;
                                    session.Character.MapInstance.Broadcast(s.GenerateRc(health));
                                    s.Mate?.Owner.Session.SendPacket(s.Mate.GenerateStatInfo());
                                });
                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                            session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 48));
                        }
                    }
                    break;

                // dignity restoration
                case 14:
                    if ((EffectValue == 100 || EffectValue == 200) && session.Character.Dignity < 100 && !session.Character.IsVehicled)
                    {
                        session.Character.Dignity += EffectValue;
                        if (session.Character.Dignity > 100)
                        {
                            session.Character.Dignity = 100;
                        }
                        session.SendPacket(session.Character.GenerateFd());
                        session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 49 - (byte)session.Character.Faction));
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    else if (EffectValue == 2000 && session.Character.Dignity < 100 && !session.Character.IsVehicled)
                    {
                        session.Character.Dignity = 100;
                        session.SendPacket(session.Character.GenerateFd());
                        session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 49 - (byte)session.Character.Faction));
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                // speakers
                case 15:
                    if (!session.Character.IsVehicled && Option == 0)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateGuri(10, 3, session.Character.CharacterId, 1));
                    }
                    break;

                // bubbles
                case 16:
                    if (!session.Character.IsVehicled && Option == 0)
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateGuri(10, 4, session.Character.CharacterId, 1));
                    }
                    break;

                // wigs
                case 30:
                    if (!session.Character.IsVehicled)
                    {
                        ItemInstance wig = session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Hat, InventoryType.Wear);
                        if (wig != null)
                        {
                            wig.Design = (byte)ServerManager.RandomNumber(0, 15);
                            session.SendPacket(session.Character.GenerateEquipment());
                            session.SendPacket(session.Character.GenerateEq());
                            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                            session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                        }
                        else
                        {
                            session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                        }
                    }
                    break;

                case 31:
                    if (!session.Character.IsVehicled && session.Character.HairStyle == HairStyleType.Hair7)
                    {
                        session.Character.HairStyle = HairStyleType.Hair8;
                        session.SendPacket(session.Character.GenerateEq());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateIn());
                        session.CurrentMapInstance?.Broadcast(session, session.Character.GenerateGidx());
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);

                        // idk how it works yet but seems like all characters with this hairstyle have DarkPurple hair
                        //session.Character.HairColor = HairColorType.DarkPurple;
                    }
                    break;

                // Passive skills books
                case 99:
                    if (session.Character.HeroLevel >= EffectValue)
                    {
                        if (session.Character.AddSkill((short)(Sex + 1)))
                        {
                            session.SendPacket(session.Character.GenerateSki());
                            session.SendPacket($"pdti 1 {Sex + 1} 0 0 0 0");
                            session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                        }
                    }
                    else
                    {
                        session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("LOW_HERO_LVL"), EffectValue), 10);
                    }
                    break;

                case 300:
                    if (session.Character.Group != null && session.Character.Group.GroupType != GroupType.Group && session.Character.Group.IsLeader(session) && session.CurrentMapInstance.Portals.Any(s => s.Type == (short)PortalType.Raid))
                    {
                        int delay = 0;
                        foreach (ClientSession sess in session.Character.Group.Sessions.GetAllItems())
                        {
                            Observable.Timer(TimeSpan.FromMilliseconds(delay)).SafeSubscribe(o =>
                            {
                                if (sess?.Character != null && session?.CurrentMapInstance != null && session?.Character != null && sess.Character != session.Character)
                                {
                                    ServerManager.Instance.TeleportOnRandomPlaceInMap(sess, session.CurrentMapInstance.MapInstanceId);
                                }
                            });
                            delay += 100;
                        }
                        session.Character.Inventory.RemoveItemFromInventory(inv.Id);
                    }
                    break;

                default:
                    Logger.Log.Warn(string.Format(Language.Instance.GetMessageFromKey("NO_HANDLER_ITEM"), GetType(), VNum, Effect, EffectValue));
                    break;
            }
        }

        #endregion
    }
}