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

using NosByte.Packets.CommandPackets;
using NosByte.Shared;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Core.Serializing;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenNos.Handler
{
    public class CommandPacketHandler : IPacketHandler
    {
        #region Instantiation

        public CommandPacketHandler(ClientSession session) => Session = session;

        #endregion

        #region Properties

        private ClientSession Session { get; }

        #endregion

        #region Methods

        /// <summary>
        /// $Exp Command
        /// </summary>
        /// <param name="expPacket"></param>
        public void Exp(ExpPacket expPacket)
        {
            void SendHelp()
            {
                Session.SendPacket(Session.Character.GenerateSay(ExpPacket.ReturnHelp(), 10));
            }

            if (expPacket == null)
            {
                SendHelp();
                return;
            }

            if (expPacket.Level == 0 || expPacket.Experience == 0)
            {
                SendHelp();
                return;
            }

            switch (expPacket.Type)
            {
                case ExpType.Xp:
                    double XpLoad = CharacterHelper.XPData[expPacket.Level - 1];
                    double percent = Math.Round((expPacket.Experience / XpLoad) * 100, 2);
                    Session.SendPacket(UserInterfaceHelper.GenerateSay($"{expPacket.Experience} experience of level {expPacket.Level} is equals to {percent}%", 12));
                    break;

                case ExpType.JXp:
                    double JXpLoad =CharacterHelper.JobXpData[(int)Session.Character.Class, expPacket.Level - 1];
                    double percent2 = Math.Round((expPacket.Experience / JXpLoad) * 100, 2);
                    Session.SendPacket(UserInterfaceHelper.GenerateSay($"{expPacket.Experience} experience of job level {expPacket.Level} is equals to {percent2}%", 12));
                    break;

                case ExpType.HXp:
                    double HXpLoad = CharacterHelper.HeroXpData[expPacket.Level - 1];
                    double percent3 = Math.Round((expPacket.Experience / HXpLoad) * 100, 2);
                    Session.SendPacket(UserInterfaceHelper.GenerateSay($"{expPacket.Experience} experience of hero level {expPacket.Level} is equals to {percent3}%", 12));
                    break;

                default:
                    SendHelp();
                    break;
            }
        }

        /// <summary>
        /// $Act4 Command
        /// </summary>
        /// <param name="act4Packet"></param>
        public void Act4(Act4Packet act4Packet)
        {
            if (act4Packet != null)
            {

                if (ServerManager.Instance.IsAct4Online())
                {
                    switch (Session.Character.Faction)
                    {
                        case FactionType.None:
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 145, 51, 41);
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("You need to be part of a faction to join Act 4"));
                            return;

                        case FactionType.Angel:
                            Session.Character.MapId = 130;
                            Session.Character.MapX = 12;
                            Session.Character.MapY = 40;
                            break;

                        case FactionType.Demon:
                            Session.Character.MapId = 131;
                            Session.Character.MapX = 12;
                            Session.Character.MapY = 40;
                            break;
                    }

                    Session.Character.ChangeChannel(ServerManager.Instance.Configuration.Act4IP, ServerManager.Instance.Configuration.Act4Port, 1);
                }
                else
                {
                    ServerManager.Instance.ChangeMap(Session.Character.CharacterId, 145, 51, 41);
                    Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("ACT4_OFFLINE")));
                }
            }

            Session.SendPacket(Session.Character.GenerateSay(Act4Packet.ReturnHelp(), 10));
        }

        public void Act4Stat(Act4StatPacket packet)
        {
            if (packet != null && ServerManager.Instance.ChannelId == 51)
            {
                ServerManager.Instance.IncreaseFcPercentage(packet.Value, packet.Faction, false, true);
                Parallel.ForEach(ServerManager.Instance.Sessions, sess => sess.SendPacket(sess.Character.GenerateFc()));
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Act4StatPacket.ReturnHelp(), 10));
            }
        }

        //// <summary>
        ///     $Act6Percent
        /// </summary>
        /// <param name="packet"></param>
        public void Act6Percent(Act6RaidPacket packet)
        {
            if (packet == null)
            {
                Session.SendPacket(Session.Character.GenerateSay("You almost crashed the server packet!", 11));
                return;
            }
            if (packet.Percent == null)
            {
                Session.SendPacket(Session.Character.GenerateSay("You almost crashed the server Percent!", 11));
                return;
            }
            if (packet.Name == null)
            {
                Session.SendPacket(Session.Character.GenerateSay("You almost crashed the server Name!", 11));
                return;
            }
            {
                if (string.IsNullOrEmpty(packet?.Name))
                {
                    Session.SendPacket(Session.Character.GenerateSay("$Act6Percent Name [Percent]", 11));
                    Session.SendPacket(Session.Character.GenerateSay("(Percent is optionnal)", 11));
                    return;
                }

                switch (packet.Name)
                {
                    case "Erenia":
                    case "erenia":
                        ServerManager.Instance.Act6Erenia.Percentage = (short)(packet.Percent.HasValue ? packet.Percent * 10 : 1000);
                        ServerManager.Instance.Act6Process();
                        Session.SendPacket(Session.Character.GenerateSay("Done !", 11));
                        break;

                    case "Zenas":
                    case "zenas":
                        ServerManager.Instance.Act6Zenas.Percentage = (short)(packet.Percent.HasValue ? packet.Percent * 10 : 1000);
                        ServerManager.Instance.Act6Process();
                        Session.SendPacket(Session.Character.GenerateSay("Done !", 11));
                        break;
                }
            }
        }

        /// <summary>
        /// $AddMonster Command
        /// </summary>
        /// <param name="addMonsterPacket"></param>
        public void AddMonster(AddMonsterPacket addMonsterPacket)
        {
            if (addMonsterPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[AddMonster]NpcMonsterVNum: {addMonsterPacket.MonsterVNum} IsMoving: {addMonsterPacket.IsMoving}");

                if (!Session.HasCurrentMapInstance)
                {
                    return;
                }

                NpcMonster npcmonster = ServerManager.GetNpcMonster(addMonsterPacket.MonsterVNum);
                if (npcmonster == null)
                {
                    return;
                }

                MapMonsterDTO monst = new MapMonsterDTO
                {
                    MonsterVNum = addMonsterPacket.MonsterVNum,
                    MapY = Session.Character.PositionY,
                    MapX = Session.Character.PositionX,
                    MapId = Session.Character.MapInstance.Map.MapId,
                    Position = Session.Character.Direction,
                    IsMoving = addMonsterPacket.IsMoving,
                    MapMonsterId = ServerManager.Instance.GetNextMobId()
                };
                if (!DAOFactory.MapMonsterDAO.DoesMonsterExist(monst.MapMonsterId))
                {
                    DAOFactory.MapMonsterDAO.Insert(monst);
                    if (DAOFactory.MapMonsterDAO.LoadById(monst.MapMonsterId) is MapMonsterDTO monsterDTO)
                    {
                        MapMonster monster = new MapMonster(monsterDTO);
                        monster.Initialize(Session.CurrentMapInstance);
                        Session.CurrentMapInstance.AddMonster(monster);
                        Session.CurrentMapInstance?.Broadcast(monster.GenerateIn());
                    }
                }

                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddMonsterPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $AddNpc Command
        /// </summary>
        /// <param name="addNpcPacket"></param>
        public void AddNpc(AddNpcPacket addNpcPacket)
        {
            if (addNpcPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[AddNpc]NpcMonsterVNum: {addNpcPacket.NpcVNum} IsMoving: {addNpcPacket.IsMoving}");

                if (!Session.HasCurrentMapInstance)
                {
                    return;
                }

                NpcMonster npcmonster = ServerManager.GetNpcMonster(addNpcPacket.NpcVNum);
                if (npcmonster == null)
                {
                    return;
                }

                MapNpcDTO newNpc = new MapNpcDTO
                {
                    NpcVNum = addNpcPacket.NpcVNum,
                    MapY = Session.Character.PositionY,
                    MapX = Session.Character.PositionX,
                    MapId = Session.Character.MapInstance.Map.MapId,
                    Position = Session.Character.Direction,
                    IsMoving = addNpcPacket.IsMoving,
                    MapNpcId = ServerManager.Instance.GetNextNpcId()
                };
                if (!DAOFactory.MapNpcDAO.DoesNpcExist(newNpc.MapNpcId))
                {
                    DAOFactory.MapNpcDAO.Insert(newNpc);
                    if (DAOFactory.MapNpcDAO.LoadById(newNpc.MapNpcId) is MapNpcDTO npcDTO)
                    {
                        MapNpc npc = new MapNpc(npcDTO);
                        npc.Initialize(Session.CurrentMapInstance);
                        Session.CurrentMapInstance.AddNPC(npc);
                        Session.CurrentMapInstance?.Broadcast(npc.GenerateIn());
                    }
                }

                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddNpcPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $AddPartner Command
        /// </summary>
        /// <param name="addPartnerPacket"></param>
        public void AddPartner(AddPartnerPacket addPartnerPacket)
        {
            if (addPartnerPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[AddPartner]NpcMonsterVNum: {addPartnerPacket.MonsterVNum} Level: {addPartnerPacket.Level}");

                AddMate(addPartnerPacket.MonsterVNum, addPartnerPacket.Level, MateType.Partner);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddPartnerPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $AddPet Command
        /// </summary>
        /// <param name="addPetPacket"></param>
        public void AddPet(AddPetPacket addPetPacket)
        {
            if (addPetPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[AddPet]NpcMonsterVNum: {addPetPacket.MonsterVNum} Level: {addPetPacket.Level}");

                AddMate(addPetPacket.MonsterVNum, addPetPacket.Level, MateType.Pet);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddPartnerPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $AddPortal Command
        /// </summary>
        /// <param name="addPortalPacket"></param>
        public void AddPortal(AddPortalPacket addPortalPacket)
        {
            if (addPortalPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[AddPortal]DestinationMapId: {addPortalPacket.DestinationMapId} DestinationMapX: {addPortalPacket.DestinationX} DestinationY: {addPortalPacket.DestinationY}");

                AddPortal(addPortalPacket.DestinationMapId, addPortalPacket.DestinationX, addPortalPacket.DestinationY,
                    addPortalPacket.PortalType == null ? (short)-1 : (short)addPortalPacket.PortalType, true);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddPortalPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $AddQuest
        /// </summary>
        /// <param name="addQuestPacket"></param>
        public void AddQuest(AddQuestPacket addQuestPacket)
        {
            if (addQuestPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                                       $"[AddQuest]QuestId: {addQuestPacket.QuestId}");

                if (ServerManager.Instance.Quests.Any(q => q.QuestId == addQuestPacket.QuestId))
                {
                    Session.Character.AddQuest(addQuestPacket.QuestId, false);
                    return;
                }

                Session.SendPacket(Session.Character.GenerateSay("This Quest doesn't exist", 11));
            }
        }

        /// <summary>
        /// $AddShellEffect Command
        /// </summary>
        /// <param name="addShellEffectPacket"></param>
        public void AddShellEffect(AddShellEffectPacket addShellEffectPacket)
        {
            if (addShellEffectPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[AddShellEffect]Slot: {addShellEffectPacket.Slot} EffectLevel: {addShellEffectPacket.EffectLevel} Effect: {addShellEffectPacket.Effect} Value: {addShellEffectPacket.Value}");

                try
                {
                    ItemInstance instance =
                        Session.Character.Inventory.LoadBySlotAndType(addShellEffectPacket.Slot,
                            InventoryType.Equipment);
                    if (instance != null)
                    {
                        var effect = addShellEffectPacket.Effect;
                        var effectLevel = addShellEffectPacket.EffectLevel;
                        if (instance.Item.ItemType == ItemType.Armor)
                        {
                            effect += 50;
                            addShellEffectPacket.EffectLevel += 12;
                        }

                        instance.ShellEffects.Add(new ShellEffectDTO
                        {
                            EffectLevel = (ShellEffectLevelType)effectLevel,
                            Effect = effect,
                            Value = addShellEffectPacket.Value,
                            EquipmentSerialId = instance.EquipmentSerialId
                        });
                    }
                }
                catch (Exception)
                {
                    Session.SendPacket(Session.Character.GenerateSay(AddShellEffectPacket.ReturnHelp(), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddShellEffectPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $AddSkill Command
        /// </summary>
        /// <param name="addSkillPacket"></param>
        public void AddSkill(AddSkillPacket addSkillPacket)
        {
            if (addSkillPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[AddSkill]SkillVNum: {addSkillPacket.SkillVNum}");

                Session.Character.AddSkill(addSkillPacket.SkillVNum);
                Session.SendPacket(Session.Character.GenerateSki());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AddSkillPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ArenaWinner Command
        /// </summary>
        /// <param name="arenaWinner"></param>
        public void ArenaWinner(ArenaWinnerPacket arenaWinner)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[ArenaWinner]");

            Session.Character.ArenaWinner = Session.Character.ArenaWinner == 0 ? 1 : 0;
            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        /// $AddMonster Command
        /// </summary>
        /// <param name="backMobPacket"></param>
        public void BackMob(BackMobPacket backMobPacket)
        {
            if (backMobPacket != null)
            {
                if (!Session.HasCurrentMapInstance)
                {
                    return;
                }

                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[BackMob]");

                object lastObject = Session.CurrentMapInstance.RemovedMobNpcList.LastOrDefault();

                if (lastObject is MapMonster mapMonster)
                {
                    MapMonsterDTO backMonst = new MapMonsterDTO
                    {
                        MonsterVNum = mapMonster.MonsterVNum,
                        MapX = mapMonster.MapX,
                        MapY = mapMonster.MapY,
                        MapId = Session.Character.MapInstance.Map.MapId,
                        Position = Session.Character.Direction,
                        IsMoving = mapMonster.IsMoving,
                        MapMonsterId = ServerManager.Instance.GetNextMobId()
                    };
                    if (!DAOFactory.MapMonsterDAO.DoesMonsterExist(backMonst.MapMonsterId))
                    {
                        DAOFactory.MapMonsterDAO.Insert(backMonst);
                        if (DAOFactory.MapMonsterDAO.LoadById(backMonst.MapMonsterId) is MapMonsterDTO monsterDTO)
                        {
                            MapMonster monster = new MapMonster(monsterDTO);
                            monster.Initialize(Session.CurrentMapInstance);
                            Session.CurrentMapInstance.AddMonster(monster);
                            Session.CurrentMapInstance?.Broadcast(monster.GenerateIn());
                            Session.CurrentMapInstance.RemovedMobNpcList.Remove(mapMonster);
                            Session.SendPacket(Session.Character.GenerateSay($"MapMonster VNum: {backMonst.MonsterVNum} recovered sucessfully", 10));
                        }
                    }
                }
                else if (lastObject is MapNpc mapNpc)
                {
                    MapNpcDTO backNpc = new MapNpcDTO
                    {
                        NpcVNum = mapNpc.NpcVNum,
                        MapX = mapNpc.MapX,
                        MapY = mapNpc.MapY,
                        MapId = Session.Character.MapInstance.Map.MapId,
                        Position = Session.Character.Direction,
                        IsMoving = mapNpc.IsMoving,
                        MapNpcId = ServerManager.Instance.GetNextMobId()
                    };
                    if (!DAOFactory.MapNpcDAO.DoesNpcExist(backNpc.MapNpcId))
                    {
                        DAOFactory.MapNpcDAO.Insert(backNpc);
                        if (DAOFactory.MapNpcDAO.LoadById(backNpc.MapNpcId) is MapNpcDTO npcDTO)
                        {
                            MapNpc npc = new MapNpc(npcDTO);
                            npc.Initialize(Session.CurrentMapInstance);
                            Session.CurrentMapInstance.AddNPC(npc);
                            Session.CurrentMapInstance?.Broadcast(npc.GenerateIn());
                            Session.CurrentMapInstance.RemovedMobNpcList.Remove(mapNpc);
                            Session.SendPacket(Session.Character.GenerateSay($"MapNpc VNum: {backNpc.NpcVNum} recovered sucessfully", 10));
                        }
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(BackMobPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Ban Command
        /// </summary>
        /// <param name="banPacket"></param>
        public void Ban(BanPacket banPacket)
        {
            if (banPacket != null)
            {
                BanMethod(banPacket.CharacterName, banPacket.Duration, banPacket.Reason);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(BanPacket.ReturnHelp(), 10));
            }
        }

        // <summary>
        /// $FUnban Command
        /// </summary>
        /// <param name="FUnbanPacket"></param>
        public void FUnban(FUnbanPacket packet)
        {
            if (string.IsNullOrEmpty(packet.CharacterName))
            {
                FUnbanPacket.ReturnHelp();
                return;
            }

            // Check if character is on current channel
            var targetSession = ServerManager.Instance.Sessions.Where(s => s.HasSelectedCharacter).FirstOrDefault(s => s.Character.Name == packet.CharacterName);

            if (targetSession != null)
            {
                targetSession.Character.LastFamilyLeave = 0;
                targetSession.SendPacket(targetSession.Character.GenerateSay("You can now join a family again.", 12));
                return;
            }

            // If character is not on current channel, check on the other channels
            var characterDto = DAOFactory.CharacterDAO.LoadByName(packet.CharacterName);

            if (characterDto == null)
            {
                Session.SendPacket(Session.Character.GenerateSay("This character doesn't exist.", 11));
                return;
            }

            var onlineCharacters = CommunicationServiceClient.Instance.RetrieveOnlineCharacters(characterDto.CharacterId);

            // If the character is not online, update it in database
            if (onlineCharacters == null || !onlineCharacters.Any())
            {
                characterDto.LastFamilyLeave = 0;
                DAOFactory.CharacterDAO.InsertOrUpdate(ref characterDto);
                return;
            }

            var charInfo = onlineCharacters.FirstOrDefault(s => s[0] == characterDto.CharacterId);

            if (charInfo == null || !charInfo.Any())
            {
                Session.SendPacket(Session.Character.GenerateSay("An error occurred while trying to edit this character.", 11));
                return;
            }
            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = characterDto.CharacterId,
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Type = MessageType.ReceivePacket,
                Message = $"$FUnban {characterDto.Name}$"
            });
        }

        /// <summary>
        /// $Bank Command
        /// </summary>
        /// <param name="bankPacket"></param>
        public void BankManagement(BankPacket bankPacket)
        {
            Session.OpenBank();
        }

        /// <summary>
        /// $BlockExp Command
        /// </summary>
        /// <param name="blockExpPacket"></param>
        public void BlockExp(BlockExpPacket blockExpPacket)
        {
            if (blockExpPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[BlockExp]CharacterName: {blockExpPacket.CharacterName} Reason: {blockExpPacket.Reason} Until: {DateTime.Now.AddMinutes(blockExpPacket.Duration)}");

                if (blockExpPacket.Duration == 0)
                {
                    blockExpPacket.Duration = 60;
                }

                blockExpPacket.Reason = blockExpPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockExpPacket.CharacterName);
                if (character != null)
                {
                    ClientSession session =
                        ServerManager.Instance.Sessions.FirstOrDefault(s =>
                            s.Character?.Name == blockExpPacket.CharacterName);
                    session?.SendPacket(blockExpPacket.Duration == 1
                        ? UserInterfaceHelper.GenerateInfo(
                            string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), blockExpPacket.Reason))
                        : UserInterfaceHelper.GenerateInfo(string.Format(
                            Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockExpPacket.Reason,
                            blockExpPacket.Duration)));
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = blockExpPacket.Reason,
                        Penalty = PenaltyType.BlockExp,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(blockExpPacket.Duration),
                        AdminName = Session.Character.Name
                    };
                    Character.InsertOrUpdatePenalty(log);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(BlockExpPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $BlockFExp Command
        /// </summary>
        /// <param name="blockFExpPacket"></param>
        public void BlockFExp(BlockFExpPacket blockFExpPacket)
        {
            if (blockFExpPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[BlockFExp]CharacterName: {blockFExpPacket.CharacterName} Reason: {blockFExpPacket.Reason} Until: {DateTime.Now.AddMinutes(blockFExpPacket.Duration)}");

                if (blockFExpPacket.Duration == 0)
                {
                    blockFExpPacket.Duration = 60;
                }

                blockFExpPacket.Reason = blockFExpPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockFExpPacket.CharacterName);
                if (character != null)
                {
                    ClientSession session =
                        ServerManager.Instance.Sessions.FirstOrDefault(s =>
                            s.Character?.Name == blockFExpPacket.CharacterName);
                    session?.SendPacket(blockFExpPacket.Duration == 1
                        ? UserInterfaceHelper.GenerateInfo(
                            string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"),
                                blockFExpPacket.Reason))
                        : UserInterfaceHelper.GenerateInfo(string.Format(
                            Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockFExpPacket.Reason,
                            blockFExpPacket.Duration)));
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = blockFExpPacket.Reason,
                        Penalty = PenaltyType.BlockFExp,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(blockFExpPacket.Duration),
                        AdminName = Session.Character.Name
                    };
                    Character.InsertOrUpdatePenalty(log);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(BlockFExpPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $BlockPM Command
        /// </summary>
        /// <param name="blockPmPacket"></param>
        public void BlockPm(BlockPMPacket blockPmPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[BlockPM]");

            if (!Session.Character.GmPvtBlock)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_BLOCK_ENABLE"),
                    10));
                Session.Character.GmPvtBlock = true;
            }
            else
            {
                Session.SendPacket(
                    Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GM_BLOCK_DISABLE"), 10));
                Session.Character.GmPvtBlock = false;
            }
        }

        /// <summary>
        /// $BlockRep Command
        /// </summary>
        /// <param name="blockRepPacket"></param>
        public void BlockRep(BlockRepPacket blockRepPacket)
        {
            if (blockRepPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[BlockRep]CharacterName: {blockRepPacket.CharacterName} Reason: {blockRepPacket.Reason} Until: {DateTime.Now.AddMinutes(blockRepPacket.Duration)}");

                if (blockRepPacket.Duration == 0)
                {
                    blockRepPacket.Duration = 60;
                }

                blockRepPacket.Reason = blockRepPacket.Reason?.Trim();
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(blockRepPacket.CharacterName);
                if (character != null)
                {
                    ClientSession session =
                        ServerManager.Instance.Sessions.FirstOrDefault(s =>
                            s.Character?.Name == blockRepPacket.CharacterName);
                    session?.SendPacket(blockRepPacket.Duration == 1
                        ? UserInterfaceHelper.GenerateInfo(
                            string.Format(Language.Instance.GetMessageFromKey("MUTED_SINGULAR"), blockRepPacket.Reason))
                        : UserInterfaceHelper.GenerateInfo(string.Format(
                            Language.Instance.GetMessageFromKey("MUTED_PLURAL"), blockRepPacket.Reason,
                            blockRepPacket.Duration)));
                    PenaltyLogDTO log = new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = blockRepPacket.Reason,
                        Penalty = PenaltyType.BlockRep,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now.AddMinutes(blockRepPacket.Duration),
                        AdminName = Session.Character.Name
                    };
                    Character.InsertOrUpdatePenalty(log);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(BlockRepPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Buff packet
        /// </summary>
        /// <param name="buffPacket"></param>
        public void Buff(BuffPacket buffPacket)
        {
            if (buffPacket != null)
            {
                Buff buff = new Buff(buffPacket.CardId, buffPacket.Level ?? (byte)1);
                Session.Character.AddBuff(buff, Session.Character.BattleEntity);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(BuffPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ChangeClass Command
        /// </summary>
        /// <param name="changeClassPacket"></param>
        public void ChangeClass(ChangeClassPacket changeClassPacket)
        {
            if (changeClassPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[ChangeClass]Class: {changeClassPacket.ClassType}");

                Session.Character.ChangeClass(changeClassPacket.ClassType, true);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeClassPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ChangeDignity Command
        /// </summary>
        /// <param name="changeDignityPacket"></param>
        public void ChangeDignity(ChangeDignityPacket changeDignityPacket)
        {
            if (changeDignityPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[ChangeDignity]Dignity: {changeDignityPacket.Dignity}");

                if (changeDignityPacket.Dignity >= -1000 && changeDignityPacket.Dignity <= 100)
                {
                    Session.Character.Dignity = changeDignityPacket.Dignity;
                    Session.SendPacket(Session.Character.GenerateFd());
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("DIGNITY_CHANGED"), 12));
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DIGNITY"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeDignityPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $FLvl Command
        /// </summary>
        /// <param name="changeFairyLevelPacket"></param>
        public void ChangeFairyLevel(ChangeFairyLevelPacket changeFairyLevelPacket)
        {
            ItemInstance fairy =
                Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (changeFairyLevelPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[FLvl]FairyLevel: {changeFairyLevelPacket.FairyLevel}");

                if (fairy != null)
                {
                    short fairylevel = changeFairyLevelPacket.FairyLevel;
                    fairylevel -= fairy.Item.ElementRate;
                    fairy.ElementRate = fairylevel;
                    fairy.XP = 0;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("FAIRY_LEVEL_CHANGED"), fairy.Item.Name),
                        10));
                    Session.SendPacket(Session.Character.GeneratePairy());
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_FAIRY"),
                        10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeFairyLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ChangeSex Command
        /// </summary>
        /// <param name="changeSexPacket"></param>
        public void ChangeGender(ChangeSexPacket changeSexPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[ChangeSex]");

            Session.Character.ChangeSex();
        }

        /// <summary>
        /// $HeroLvl Command
        /// </summary>
        /// <param name="changeHeroLevelPacket"></param>
        public void ChangeHeroLevel(ChangeHeroLevelPacket changeHeroLevelPacket)
        {
            if (changeHeroLevelPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[HeroLvl]HeroLevel: {changeHeroLevelPacket.HeroLevel}");

                if (changeHeroLevelPacket.HeroLevel <= 255)
                {
                    Session.Character.HeroLevel = changeHeroLevelPacket.HeroLevel;
                    Session.Character.HeroXp = 0;
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("HEROLEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPackets(Session.Character.GenerateStatChar());
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(),
                        ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(),
                        ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 6),
                        Session.Character.PositionX, Session.Character.PositionY);
                    Session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 198),
                        Session.Character.PositionX, Session.Character.PositionY);
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeHeroLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $JLvl Command
        /// </summary>
        /// <param name="changeJobLevelPacket"></param>
        public void ChangeJobLevel(ChangeJobLevelPacket changeJobLevelPacket)
        {
            if (changeJobLevelPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[JLvl]JobLevel: {changeJobLevelPacket.JobLevel}");

                if (((Session.Character.Class == 0 && changeJobLevelPacket.JobLevel <= 20)
                    || (Session.Character.Class != 0 && changeJobLevelPacket.JobLevel <= 255))
                    && changeJobLevelPacket.JobLevel > 0)
                {
                    Session.Character.JobLevel = changeJobLevelPacket.JobLevel;
                    Session.Character.JobLevelXp = 0;
                    Session.Character.ResetSkills();
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("JOBLEVEL_CHANGED"), 0));
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 8), Session.Character.PositionX, Session.Character.PositionY);
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeJobLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Lvl Command
        /// </summary>
        /// <param name="changeLevelPacket"></param>
        public void ChangeLevel(ChangeLevelPacket changeLevelPacket)
        {
            if (changeLevelPacket != null && !Session.Character.IsSeal)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Lvl]Level: {changeLevelPacket.Level}");

                if (changeLevelPacket.Level > 0)
                {
                    Session.Character.Level = Math.Min(changeLevelPacket.Level,
                        ServerManager.Instance.Configuration.MaxLevel);
                    Session.Character.LevelXp = 0;
                    Session.Character.Hp = (int)Session.Character.HPLoad();
                    Session.Character.Mp = (int)Session.Character.MPLoad();
                    Session.SendPacket(Session.Character.GenerateStat());
                    Session.SendPackets(Session.Character.GenerateStatChar());
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LEVEL_CHANGED"), 0));
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(),
                        ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(),
                        ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 6),
                        Session.Character.PositionX, Session.Character.PositionY);
                    Session.CurrentMapInstance?.Broadcast(
                        StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 198),
                        Session.Character.PositionX, Session.Character.PositionY);
                    ServerManager.Instance.UpdateGroup(Session.Character.CharacterId);
                    if (Session.Character.Family != null)
                    {
                        ServerManager.Instance.FamilyRefresh(Session.Character.Family.FamilyId);
                        CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                        {
                            DestinationCharacterId = Session.Character.Family.FamilyId,
                            SourceCharacterId = Session.Character.CharacterId,
                            SourceWorldId = ServerManager.Instance.WorldId,
                            Message = "fhis_stc",
                            Type = MessageType.Family
                        });
                    }
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ChangeRep Command
        /// </summary>
        /// <param name="changeReputationPacket"></param>
        public void ChangeReputation(ChangeReputationPacket changeReputationPacket)
        {
            if (changeReputationPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[ChangeRep]Reputation: {changeReputationPacket.Reputation}");

                if (changeReputationPacket.Reputation > 0)
                {
                    Session.Character.Reputation = changeReputationPacket.Reputation;
                    Session.SendPacket(Session.Character.GenerateFd());
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("REP_CHANGED"), 0));
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(broadcastEffect: 1), ReceiverType.AllExceptMe);
                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(), ReceiverType.AllExceptMe);
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeReputationPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ChangeShopName Packet
        /// </summary>
        /// <param name="changeShopNamePacket"></param>
        public void ChangeShopName(ChangeShopNamePacket changeShopNamePacket)
        {
            if (Session.HasCurrentMapInstance)
            {
                if (!string.IsNullOrEmpty(changeShopNamePacket.Name))
                {
                    if (Session.CurrentMapInstance.GetNpc(Session.Character.LastNpcMonsterId) is MapNpc npc)
                    {
                        if (npc.Shop is Shop shop)
                        {
                            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                                $"[ChangeShopName]ShopId: {shop.ShopId} Name: {changeShopNamePacket.Name}");

                            if (DAOFactory.ShopDAO.LoadById(shop.ShopId) is ShopDTO shopDTO)
                            {
                                shop.Name = changeShopNamePacket.Name;
                                shopDTO.Name = changeShopNamePacket.Name;
                                DAOFactory.ShopDAO.Update(ref shopDTO);

                                Session.CurrentMapInstance.Broadcast($"shop 2 {npc.MapNpcId} {npc.Shop.ShopId} {npc.Shop.MenuType} {npc.Shop.ShopType} {npc.Shop.Name}");
                            }
                        }
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NPCMONSTER_NOT_FOUND"), 11));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(ChangeShopNamePacket.ReturnHelp(), 10));
                }
            }
        }

        /// <summary>
        /// $SPLvl Command
        /// </summary>
        /// <param name="changeSpecialistLevelPacket"></param>
        public void ChangeSpecialistLevel(ChangeSpecialistLevelPacket changeSpecialistLevelPacket)
        {
            if (changeSpecialistLevelPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[SPLvl]SpecialistLevel: {changeSpecialistLevelPacket.SpecialistLevel}");

                ItemInstance sp =
                    Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
                if (sp != null && Session.Character.UseSp)
                {
                    if (changeSpecialistLevelPacket.SpecialistLevel <= 255
                        && changeSpecialistLevelPacket.SpecialistLevel > 0)
                    {
                        sp.SpLevel = changeSpecialistLevelPacket.SpecialistLevel;
                        sp.XP = 0;
                        Session.SendPacket(Session.Character.GenerateLev());
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SPLEVEL_CHANGED"), 0));
                        Session.Character.LearnSPSkill();
                        Session.SendPacket(Session.Character.GenerateSki());
                        Session.SendPackets(Session.Character.GenerateQuicklist());
                        Session.Character.SkillsSp.ForEach(s => s.LastUse = DateTime.Now.AddDays(-1));
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(broadcastEffect: 1),
                            ReceiverType.AllExceptMe);
                        Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(),
                            ReceiverType.AllExceptMe);
                        Session.CurrentMapInstance?.Broadcast(
                            StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 8),
                            Session.Character.PositionX, Session.Character.PositionY);
                    }
                    else
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                    }
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_SP"),
                        0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ChangeSpecialistLevelPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ChannelInfo Command
        /// </summary>
        /// <param name="channelInfoPacket"></param>
        public void ChannelInfo(ChannelInfoPacket channelInfoPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[ChannelInfo]");

            Session.SendPacket(Session.Character.GenerateSay(
                $"-----------Channel Info-----------\n-------------Channel:{ServerManager.Instance.ChannelId}-------------",
                11));
            foreach (ClientSession session in ServerManager.Instance.Sessions)
            {
                Session.SendPacket(
                    Session.Character.GenerateSay(
                        $"CharacterName: {session.Character.Name} | CharacterId: {session.Character.CharacterId} | SessionId: {session.SessionId}", 12));
            }

            Session.SendPacket(Session.Character.GenerateSay("----------------------------------------", 11));
        }

        /// <summary>
        /// $PlayerList Command
        /// </summary>
        /// <param name="playerListPacket"></param>
        public void PlayerList(PlayerListPacket playerListPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[ChannelInfo]");

            Session.SendPacket(Session.Character.GenerateSay(
                $"-----------Player List-----------\n-------------Channel:{ServerManager.Instance.ChannelId}-------------",
                11));
            foreach (ClientSession session in ServerManager.Instance.Sessions)
            {
                Session.SendPacket(
                    Session.Character.GenerateSay(
                        $"Name: {session.Character.Name} | Level: {session.Character.Level}+{session.Character.HeroLevel}| MapId: {session.Character.MapId} X: {session.Character.MapX} Y: {session.Character.MapY}", 12));
            }

            Session.SendPacket(Session.Character.GenerateSay("----------------------------------------", 11));
        }

        /// <summary>
        /// $CharEdit Command
        /// </summary>
        /// <param name="characterEditPacket"></param>
        public void CharacterEdit(CharacterEditPacket characterEditPacket)
        {
            if (characterEditPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[CharEdit]Property: {characterEditPacket.Property} Value: {characterEditPacket.Data}");

                if (characterEditPacket.Property != null && !string.IsNullOrEmpty(characterEditPacket.Data))
                {
                    PropertyInfo propertyInfo = Session.Character.GetType().GetProperty(characterEditPacket.Property);
                    if (propertyInfo != null)
                    {
                        propertyInfo.SetValue(Session.Character,
                            Convert.ChangeType(characterEditPacket.Data, propertyInfo.PropertyType));
                        ServerManager.Instance.ChangeMap(Session.Character.CharacterId);
                        Session.Character.Save();
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"),
                            10));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(CharacterEditPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $CharStat Command
        /// </summary>
        /// <param name="characterStatsPacket"></param>
        public void CharStat(CharacterStatsPacket characterStatsPacket)
        {
            string returnHelp = CharacterStatsPacket.ReturnHelp();
            if (characterStatsPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[CharStat]CharacterName: {characterStatsPacket.CharacterName}");

                string name = characterStatsPacket.CharacterName;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (ServerManager.Instance.GetSessionByCharacterName(name) != null)
                    {
                        Character character = ServerManager.Instance.GetSessionByCharacterName(name).Character;
                        SendStats(character);
                    }
                    else if (DAOFactory.CharacterDAO.LoadByName(name) != null)
                    {
                        CharacterDTO characterDto = DAOFactory.CharacterDAO.LoadByName(name);
                        SendStats(characterDto);
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(returnHelp, 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(returnHelp, 10));
            }
        }

        /// <summary>
        /// $ClassPack
        /// </summary>
        /// <param name="classPackPacket"></param>
        public void ClassPack(ClassPackPacket classPackPacket)
        {
            if (classPackPacket != null)
            {
                if (classPackPacket.Class < 1 || classPackPacket.Class > 3)
                {
                    Session.SendPacket(Session.Character.GenerateSay("Invalid class", 11));
                    Session.SendPacket(Session.Character.GenerateSay(ClassPackPacket.ReturnHelp(), 10));
                    return;
                }

                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                                       $"[ClassPack]Class: {classPackPacket.Class}");

                switch (classPackPacket.Class)
                {
                    case 1:
                        Session.Character.Inventory.AddNewToInventory(4075, 1);
                        Session.Character.Inventory.AddNewToInventory(4076, 1);
                        Session.Character.Inventory.AddNewToInventory(4129, 1);
                        Session.Character.Inventory.AddNewToInventory(4130, 1);
                        Session.Character.Inventory.AddNewToInventory(4131, 1);
                        Session.Character.Inventory.AddNewToInventory(4132, 1);
                        Session.Character.Inventory.AddNewToInventory(1685, 999);
                        Session.Character.Inventory.AddNewToInventory(1686, 999);
                        Session.Character.Inventory.AddNewToInventory(5087, 999);
                        Session.Character.Inventory.AddNewToInventory(5203, 999);
                        Session.Character.Inventory.AddNewToInventory(5372, 999);
                        Session.Character.Inventory.AddNewToInventory(5431, 999);
                        Session.Character.Inventory.AddNewToInventory(5432, 999);
                        Session.Character.Inventory.AddNewToInventory(5498, 999);
                        Session.Character.Inventory.AddNewToInventory(5499, 999);
                        Session.Character.Inventory.AddNewToInventory(5553, 999);
                        Session.Character.Inventory.AddNewToInventory(5560, 999);
                        Session.Character.Inventory.AddNewToInventory(5591, 999);
                        Session.Character.Inventory.AddNewToInventory(5837, 999);
                        Session.Character.Inventory.AddNewToInventory(4875, 1, upgrade: 14);
                        Session.Character.Inventory.AddNewToInventory(4873, 1, upgrade: 14);
                        Session.Character.Inventory.AddNewToInventory(1012, 999);
                        Session.Character.Inventory.AddNewToInventory(1012, 999);
                        Session.Character.Inventory.AddNewToInventory(1244, 999);
                        Session.Character.Inventory.AddNewToInventory(1244, 999);
                        Session.Character.Inventory.AddNewToInventory(2072, 999);
                        Session.Character.Inventory.AddNewToInventory(2071, 999);
                        Session.Character.Inventory.AddNewToInventory(2070, 999);
                        Session.Character.Inventory.AddNewToInventory(2160, 999);
                        Session.Character.Inventory.AddNewToInventory(4138, 1);
                        Session.Character.Inventory.AddNewToInventory(4146, 1);
                        Session.Character.Inventory.AddNewToInventory(4142, 1);
                        Session.Character.Inventory.AddNewToInventory(4150, 1);
                        Session.Character.Inventory.AddNewToInventory(4353, 1);
                        Session.Character.Inventory.AddNewToInventory(4124, 1);
                        Session.Character.Inventory.AddNewToInventory(4172, 1);
                        Session.Character.Inventory.AddNewToInventory(4183, 1);
                        Session.Character.Inventory.AddNewToInventory(4187, 1);
                        Session.Character.Inventory.AddNewToInventory(4283, 1);
                        Session.Character.Inventory.AddNewToInventory(4285, 1);
                        Session.Character.Inventory.AddNewToInventory(4177, 1);
                        Session.Character.Inventory.AddNewToInventory(4179, 1);
                        Session.Character.Inventory.AddNewToInventory(4244, 1);
                        Session.Character.Inventory.AddNewToInventory(4252, 1);
                        Session.Character.Inventory.AddNewToInventory(4256, 1);
                        Session.Character.Inventory.AddNewToInventory(4248, 1);
                        Session.Character.Inventory.AddNewToInventory(3116, 1);
                        Session.Character.Inventory.AddNewToInventory(1277, 999);
                        Session.Character.Inventory.AddNewToInventory(1274, 999);
                        Session.Character.Inventory.AddNewToInventory(1280, 999);
                        Session.Character.Inventory.AddNewToInventory(2419, 999);
                        Session.Character.Inventory.AddNewToInventory(1914, 1);
                        Session.Character.Inventory.AddNewToInventory(1296, 999);
                        Session.Character.Inventory.AddNewToInventory(5916, 999);
                        Session.Character.Inventory.AddNewToInventory(3001, 1);
                        Session.Character.Inventory.AddNewToInventory(3003, 1);
                        Session.Character.Inventory.AddNewToInventory(4490, 1);
                        Session.Character.Inventory.AddNewToInventory(4699, 1);
                        Session.Character.Inventory.AddNewToInventory(4099, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(900, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(907, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(908, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4883, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4889, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4895, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4371, 1);
                        Session.Character.Inventory.AddNewToInventory(4353, 1);
                        Session.Character.Inventory.AddNewToInventory(4277, 1);
                        Session.Character.Inventory.AddNewToInventory(4309, 1);
                        Session.Character.Inventory.AddNewToInventory(4271, 1);
                        Session.Character.Inventory.AddNewToInventory(901, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(902, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(909, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(910, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4500, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4497, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4493, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4489, 1, upgrade: 15);
                        break;

                    case 2:
                        Session.Character.Inventory.AddNewToInventory(4075, 1);
                        Session.Character.Inventory.AddNewToInventory(4076, 1);
                        Session.Character.Inventory.AddNewToInventory(4129, 1);
                        Session.Character.Inventory.AddNewToInventory(4130, 1);
                        Session.Character.Inventory.AddNewToInventory(4131, 1);
                        Session.Character.Inventory.AddNewToInventory(4132, 1);
                        Session.Character.Inventory.AddNewToInventory(1685, 999);
                        Session.Character.Inventory.AddNewToInventory(1686, 999);
                        Session.Character.Inventory.AddNewToInventory(5087, 999);
                        Session.Character.Inventory.AddNewToInventory(5203, 999);
                        Session.Character.Inventory.AddNewToInventory(5372, 999);
                        Session.Character.Inventory.AddNewToInventory(5431, 999);
                        Session.Character.Inventory.AddNewToInventory(5432, 999);
                        Session.Character.Inventory.AddNewToInventory(5498, 999);
                        Session.Character.Inventory.AddNewToInventory(5499, 999);
                        Session.Character.Inventory.AddNewToInventory(5553, 999);
                        Session.Character.Inventory.AddNewToInventory(5560, 999);
                        Session.Character.Inventory.AddNewToInventory(5591, 999);
                        Session.Character.Inventory.AddNewToInventory(5837, 999);
                        Session.Character.Inventory.AddNewToInventory(4875, 1, upgrade: 14);
                        Session.Character.Inventory.AddNewToInventory(4873, 1, upgrade: 14);
                        Session.Character.Inventory.AddNewToInventory(1012, 999);
                        Session.Character.Inventory.AddNewToInventory(1012, 999);
                        Session.Character.Inventory.AddNewToInventory(1244, 999);
                        Session.Character.Inventory.AddNewToInventory(1244, 999);
                        Session.Character.Inventory.AddNewToInventory(2072, 999);
                        Session.Character.Inventory.AddNewToInventory(2071, 999);
                        Session.Character.Inventory.AddNewToInventory(2070, 999);
                        Session.Character.Inventory.AddNewToInventory(2160, 999);
                        Session.Character.Inventory.AddNewToInventory(4138, 1);
                        Session.Character.Inventory.AddNewToInventory(4146, 1);
                        Session.Character.Inventory.AddNewToInventory(4142, 1);
                        Session.Character.Inventory.AddNewToInventory(4150, 1);
                        Session.Character.Inventory.AddNewToInventory(4353, 1);
                        Session.Character.Inventory.AddNewToInventory(4124, 1);
                        Session.Character.Inventory.AddNewToInventory(4172, 1);
                        Session.Character.Inventory.AddNewToInventory(4183, 1);
                        Session.Character.Inventory.AddNewToInventory(4187, 1);
                        Session.Character.Inventory.AddNewToInventory(4283, 1);
                        Session.Character.Inventory.AddNewToInventory(4285, 1);
                        Session.Character.Inventory.AddNewToInventory(4177, 1);
                        Session.Character.Inventory.AddNewToInventory(4179, 1);
                        Session.Character.Inventory.AddNewToInventory(4244, 1);
                        Session.Character.Inventory.AddNewToInventory(4252, 1);
                        Session.Character.Inventory.AddNewToInventory(4256, 1);
                        Session.Character.Inventory.AddNewToInventory(4248, 1);
                        Session.Character.Inventory.AddNewToInventory(3116, 1);
                        Session.Character.Inventory.AddNewToInventory(1277, 999);
                        Session.Character.Inventory.AddNewToInventory(1274, 999);
                        Session.Character.Inventory.AddNewToInventory(1280, 999);
                        Session.Character.Inventory.AddNewToInventory(2419, 999);
                        Session.Character.Inventory.AddNewToInventory(1914, 1);
                        Session.Character.Inventory.AddNewToInventory(1296, 999);
                        Session.Character.Inventory.AddNewToInventory(5916, 999);
                        Session.Character.Inventory.AddNewToInventory(3001, 1);
                        Session.Character.Inventory.AddNewToInventory(3003, 1);
                        Session.Character.Inventory.AddNewToInventory(4490, 1);
                        Session.Character.Inventory.AddNewToInventory(4699, 1);
                        Session.Character.Inventory.AddNewToInventory(4099, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(900, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(907, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(908, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4885, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4890, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4897, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4372, 1);
                        Session.Character.Inventory.AddNewToInventory(4310, 1);
                        Session.Character.Inventory.AddNewToInventory(4354, 1);
                        Session.Character.Inventory.AddNewToInventory(4279, 1);
                        Session.Character.Inventory.AddNewToInventory(4273, 1);
                        Session.Character.Inventory.AddNewToInventory(903, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(904, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(911, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(912, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4501, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4498, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4488, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4492, 1, upgrade: 15);
                        break;

                    case 3:
                        Session.Character.Inventory.AddNewToInventory(4075, 1);
                        Session.Character.Inventory.AddNewToInventory(4076, 1);
                        Session.Character.Inventory.AddNewToInventory(4129, 1);
                        Session.Character.Inventory.AddNewToInventory(4130, 1);
                        Session.Character.Inventory.AddNewToInventory(4131, 1);
                        Session.Character.Inventory.AddNewToInventory(4132, 1);
                        Session.Character.Inventory.AddNewToInventory(1685, 999);
                        Session.Character.Inventory.AddNewToInventory(1686, 999);
                        Session.Character.Inventory.AddNewToInventory(5087, 999);
                        Session.Character.Inventory.AddNewToInventory(5203, 999);
                        Session.Character.Inventory.AddNewToInventory(5372, 999);
                        Session.Character.Inventory.AddNewToInventory(5431, 999);
                        Session.Character.Inventory.AddNewToInventory(5432, 999);
                        Session.Character.Inventory.AddNewToInventory(5498, 999);
                        Session.Character.Inventory.AddNewToInventory(5499, 999);
                        Session.Character.Inventory.AddNewToInventory(5553, 999);
                        Session.Character.Inventory.AddNewToInventory(5560, 999);
                        Session.Character.Inventory.AddNewToInventory(5591, 999);
                        Session.Character.Inventory.AddNewToInventory(5837, 999);
                        Session.Character.Inventory.AddNewToInventory(4875, 1, upgrade: 14);
                        Session.Character.Inventory.AddNewToInventory(4873, 1, upgrade: 14);
                        Session.Character.Inventory.AddNewToInventory(1012, 999);
                        Session.Character.Inventory.AddNewToInventory(1012, 999);
                        Session.Character.Inventory.AddNewToInventory(1244, 999);
                        Session.Character.Inventory.AddNewToInventory(1244, 999);
                        Session.Character.Inventory.AddNewToInventory(2072, 999);
                        Session.Character.Inventory.AddNewToInventory(2071, 999);
                        Session.Character.Inventory.AddNewToInventory(2070, 999);
                        Session.Character.Inventory.AddNewToInventory(2160, 999);
                        Session.Character.Inventory.AddNewToInventory(4138, 1);
                        Session.Character.Inventory.AddNewToInventory(4146, 1);
                        Session.Character.Inventory.AddNewToInventory(4142, 1);
                        Session.Character.Inventory.AddNewToInventory(4150, 1);
                        Session.Character.Inventory.AddNewToInventory(4353, 1);
                        Session.Character.Inventory.AddNewToInventory(4124, 1);
                        Session.Character.Inventory.AddNewToInventory(4172, 1);
                        Session.Character.Inventory.AddNewToInventory(4183, 1);
                        Session.Character.Inventory.AddNewToInventory(4187, 1);
                        Session.Character.Inventory.AddNewToInventory(4283, 1);
                        Session.Character.Inventory.AddNewToInventory(4285, 1);
                        Session.Character.Inventory.AddNewToInventory(4177, 1);
                        Session.Character.Inventory.AddNewToInventory(4179, 1);
                        Session.Character.Inventory.AddNewToInventory(4244, 1);
                        Session.Character.Inventory.AddNewToInventory(4252, 1);
                        Session.Character.Inventory.AddNewToInventory(4256, 1);
                        Session.Character.Inventory.AddNewToInventory(4248, 1);
                        Session.Character.Inventory.AddNewToInventory(3116, 1);
                        Session.Character.Inventory.AddNewToInventory(1277, 999);
                        Session.Character.Inventory.AddNewToInventory(1274, 999);
                        Session.Character.Inventory.AddNewToInventory(1280, 999);
                        Session.Character.Inventory.AddNewToInventory(2419, 999);
                        Session.Character.Inventory.AddNewToInventory(1914, 1);
                        Session.Character.Inventory.AddNewToInventory(1296, 999);
                        Session.Character.Inventory.AddNewToInventory(5916, 999);
                        Session.Character.Inventory.AddNewToInventory(3001, 1);
                        Session.Character.Inventory.AddNewToInventory(3003, 1);
                        Session.Character.Inventory.AddNewToInventory(4490, 1);
                        Session.Character.Inventory.AddNewToInventory(4699, 1);
                        Session.Character.Inventory.AddNewToInventory(4099, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(900, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(907, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(908, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4887, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4892, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4899, 1, null, 7, 10);
                        Session.Character.Inventory.AddNewToInventory(4311, 1);
                        Session.Character.Inventory.AddNewToInventory(4373, 1);
                        Session.Character.Inventory.AddNewToInventory(4281, 1);
                        Session.Character.Inventory.AddNewToInventory(4355, 1);
                        Session.Character.Inventory.AddNewToInventory(4275, 1);
                        Session.Character.Inventory.AddNewToInventory(905, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(906, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(913, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(914, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4502, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4499, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4491, 1, upgrade: 15);
                        Session.Character.Inventory.AddNewToInventory(4487, 1, upgrade: 15);
                        break;
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ClassPackPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Clear Command
        /// </summary>
        /// <param name="clearInventoryPacket"></param>
        public void ClearInventory(ClearInventoryPacket clearInventoryPacket)
        {
            if (clearInventoryPacket != null && clearInventoryPacket.InventoryType != InventoryType.Wear)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Clear]InventoryType: {clearInventoryPacket.InventoryType}");

                Parallel.ForEach(Session.Character.Inventory.Values.Where(s => s.Type == clearInventoryPacket.InventoryType),
                    inv =>
                    {
                        Session.Character.Inventory.DeleteById(inv.Id);
                        Session.SendPacket(UserInterfaceHelper.Instance.GenerateInventoryRemove(inv.Type, inv.Slot));
                    });
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ClearInventoryPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ClearMap packet
        /// </summary>
        /// <param name="clearMapPacket"></param>
        public void ClearMap(ClearMapPacket clearMapPacket)
        {
            if (clearMapPacket != null && Session.HasCurrentMapInstance)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[ClearMap]MapId: {Session.CurrentMapInstance.MapInstanceId}");

                Parallel.ForEach(Session.CurrentMapInstance.Monsters.Where(s => s.ShouldRespawn != true), monster =>
                {
                    Session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster,
                        monster.MapMonsterId));
                    monster.SetDeathStatement();
                    Session.CurrentMapInstance.RemoveMonster(monster);
                });
                Parallel.ForEach(Session.CurrentMapInstance.DroppedList.Values, drop =>
                {
                    Session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Object, drop.TransportId));
                    Session.CurrentMapInstance.DroppedList.TryRemove(drop.TransportId, out _);
                });
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ClearMapPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Clone Command
        /// </summary>
        /// <param name="cloneItemPacket"></param>
        public void CloneItem(CloneItemPacket cloneItemPacket)
        {
            if (cloneItemPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Clone]Slot: {cloneItemPacket.Slot}");

                ItemInstance item =
                    Session.Character.Inventory.LoadBySlotAndType(cloneItemPacket.Slot, InventoryType.Equipment);
                if (item != null)
                {
                    item = item.DeepCopy();
                    item.Id = Guid.NewGuid();
                    Session.Character.Inventory.AddToInventory(item);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(CloneItemPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Help Command
        /// </summary>
        /// <param name="helpPacket"></param>
        public void Command(HelpPacket helpPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[Help]");

            // get commands
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes());

            var classes = assemblies.Where(s => s.Namespace != null &&
                s.IsClass && (s.Namespace.Contains("NosByte.Packets.CommandPackets") || s.Namespace == "NosByte.Packets.Lock") &&
                s.GetCustomAttribute<PacketHeaderAttribute>()?.Authority <= Session.Account.Authority)?.ToList();
            List<string> messages = new List<string>();
            foreach (Type type in classes)
            {
                object classInstance = Activator.CreateInstance(type);
                Type classType = classInstance.GetType();
                MethodInfo method = classType.GetMethod("ReturnHelp");
                if (method != null)
                {
                    messages.Add(method.Invoke(classInstance, null).ToString());
                }
            }

            // send messages
            messages.Sort();
            if (helpPacket.Contents == "*" || string.IsNullOrEmpty(helpPacket.Contents))
            {
                Session.SendPacket(Session.Character.GenerateSay("-------------Commands Info-------------", 11));
                foreach (string message in messages)
                {
                    Session.SendPacket(Session.Character.GenerateSay(message, 12));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay("-------------Command Info-------------", 11));
                foreach (string message in messages.Where(s =>
                    s.IndexOf(helpPacket.Contents, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    Session.SendPacket(Session.Character.GenerateSay(message, 12));
                }
            }

            Session.SendPacket(Session.Character.GenerateSay("-----------------------------------------------", 11));
        }

        /// <summary>
        /// $CreateItem Packet
        /// </summary>
        /// <param name="createItemPacket"></param>
        public void CreateItem(CreateItemPacket createItemPacket)
        {
            if (createItemPacket != null)
            {

                short vnum = createItemPacket.VNum;
                short amount = 1;
                sbyte rare = 0;
                byte upgrade = 0, design = 0;
                if (vnum == 1046)
                {
                    return; // cannot create gold as item, use $Gold instead
                }

                Item iteminfo = ServerManager.GetItem(vnum);
                if (iteminfo != null)
                {
                    if (iteminfo.IsColored || (iteminfo.ItemType == ItemType.Box && iteminfo.ItemSubType == 3))
                    {
                        if (createItemPacket.Design.HasValue)
                        {
                            rare = (sbyte)ServerManager.RandomNumber();
                            if (rare > 90)
                            {
                                rare = 7;
                            }
                            else if (rare > 80)
                            {
                                rare = 6;
                            }
                            else
                            {
                                rare = (sbyte)ServerManager.RandomNumber(1, 6);
                            }
                            design = (byte)createItemPacket.Design.Value;
                        }

                        if (createItemPacket.Upgrade.HasValue)
                        {
                            rare = (sbyte) createItemPacket.Upgrade.Value;
                            if (rare > 8)
                            {
                                rare = 8;
                            }

                            if (rare < -2)
                            {
                                rare = 0;
                            }
                        }
                    }
                    else if (iteminfo.Type == 0)
                    {
                        if (createItemPacket.Upgrade.HasValue)
                        {
                            if (iteminfo.EquipmentSlot != EquipmentType.Sp)
                            {
                                upgrade = createItemPacket.Upgrade.Value;
                            }
                            else
                            {
                                design = createItemPacket.Upgrade.Value;
                            }

                            if (iteminfo.EquipmentSlot != EquipmentType.Sp && upgrade == 0
                                && iteminfo.BasicUpgrade != 0)
                            {
                                upgrade = iteminfo.BasicUpgrade;
                            }
                        }

                        if (createItemPacket.Design.HasValue)
                        {
                            if (iteminfo.EquipmentSlot == EquipmentType.Sp)
                            {
                                upgrade = (byte)createItemPacket.Design.Value;
                            }
                            else
                            {
                                rare = (sbyte)createItemPacket.Design.Value;
                            }
                        }
                    }

                    if (createItemPacket.Design.HasValue && !createItemPacket.Upgrade.HasValue)
                    {
                        amount = createItemPacket.Design.Value > 9999 ? (short)9999 : createItemPacket.Design.Value;
                    }

                    ItemInstance inv = Session.Character.Inventory
                        .AddNewToInventory(vnum, amount, rare: rare, upgrade: upgrade, design: design).FirstOrDefault();
                    if (inv != null)
                    {
                        ItemInstance wearable = Session.Character.Inventory.LoadBySlotAndType(inv.Slot, inv.Type);
                        if (wearable != null)
                        {
                            switch (wearable.Item.EquipmentSlot)
                            {
                                case EquipmentType.Armor:
                                case EquipmentType.MainWeapon:
                                case EquipmentType.SecondaryWeapon:
                                    wearable.SetRarityPoint();
                                    if (wearable.Item.IsHeroic)
                                    {
                                        wearable.ShellEffects.Clear();
                                        DAOFactory.ShellEffectDAO.DeleteByEquipmentSerialId(wearable.EquipmentSerialId);
                                        if (inv.Item.ItemType == ItemType.Shell)
                                        {
                                            var shellType = ShellGeneratorHelper.Instance.ShellTypes[inv.Item.VNum];
                                            ShellGeneratorHelper.Instance.GenerateShell(shellType, inv.Rare == 8 ? 7 : inv.Rare, 106);
                                        }
                                        else
                                        {
                                            wearable.GenerateHeroicShell(Session, RarifyProtection.RandomHeroicAmulet);
                                        }
                                    }
                                    break;

                                case EquipmentType.Boots:
                                case EquipmentType.Gloves:
                                    wearable.FireResistance = (short)(wearable.Item.FireResistance * upgrade);
                                    wearable.DarkResistance = (short)(wearable.Item.DarkResistance * upgrade);
                                    wearable.LightResistance = (short)(wearable.Item.LightResistance * upgrade);
                                    wearable.WaterResistance = (short)(wearable.Item.WaterResistance * upgrade);
                                    break;
                            }
                        }

                        Session.SendPacket(Session.Character.GenerateSay(
                            $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {iteminfo.Name} x {amount}", 12));
                    }
                    else
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"),
                                0));
                    }
                }
                else
                {
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_ITEM"), 0);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(CreateItemPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $CustomNpcMonsterName Packet
        /// </summary>
        /// <param name="changeNpcMonsterNamePacket"></param>
        public void CustomNpcMonsterName(ChangeNpcMonsterNamePacket changeNpcMonsterNamePacket)
        {
            if (Session.HasCurrentMapInstance)
            {
                if (Session.CurrentMapInstance.GetNpc(Session.Character.LastNpcMonsterId) is MapNpc npc)
                {
                    Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                        $"[CustomNpcName]MapNpcId: {npc.MapNpcId} Name: {changeNpcMonsterNamePacket.Name}");

                    if (DAOFactory.MapNpcDAO.LoadById(npc.MapNpcId) is MapNpcDTO npcDTO)
                    {
                        npc.Name = changeNpcMonsterNamePacket.Name;
                        npcDTO.Name = changeNpcMonsterNamePacket.Name;
                        DAOFactory.MapNpcDAO.Update(ref npcDTO);

                        Session.CurrentMapInstance.Broadcast(npc.GenerateIn());
                    }
                }
                else if (Session.CurrentMapInstance.GetMonsterById(Session.Character.LastNpcMonsterId) is MapMonster monster)
                {
                    Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                        $"[CustomNpcName]MapMonsterId: {monster.MapMonsterId} Name: {changeNpcMonsterNamePacket.Name}");

                    if (DAOFactory.MapMonsterDAO.LoadById(monster.MapMonsterId) is MapMonsterDTO monsterDTO)
                    {
                        monster.Name = changeNpcMonsterNamePacket.Name;
                        monsterDTO.Name = changeNpcMonsterNamePacket.Name;
                        DAOFactory.MapMonsterDAO.Update(ref monsterDTO);

                        Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NPCMONSTER_NOT_FOUND"), 11));
                }
            }
        }

        /// <summary>
        /// $Demote Command
        /// </summary>
        /// <param name="demotePacket"></param>
        public void Demote(DemotePacket demotePacket)
        {
            if (demotePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Demote]CharacterName: {demotePacket.CharacterName}");

                string name = demotePacket.CharacterName;
                try
                {
                    AccountDTO account = ServerManager.Instance.Sessions.FirstOrDefault(s => s.HasSelectedCharacter && s.Character.Name == name)?.Account;

                    if (account == null)
                    {
                        account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(name).AccountId);
                    }

                    if (account?.Authority > AuthorityType.User)
                    {
                        if (Session.Account.Authority >= account?.Authority)
                        {
                            account.Authority = AuthorityType.User;
                            DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                            ClientSession session =
                                ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);

                            if (session != null)
                            {
                                if (demotePacket?.CharacterName == Session?.Character?.Name)
                                {
                                    Session.SendPacket("info Can't demote yourself.");
                                    return;
                                }

                                session.Account.Authority = AuthorityType.User;
                                if (session.Character.InvisibleGm)
                                {
                                    session.Character.Invisible = false;
                                    session.Character.InvisibleGm = false;
                                    Session.Character.Mates.Where(m => m.IsTeamMember).ToList().ForEach(m =>
                                        Session.CurrentMapInstance?.Broadcast(m.GenerateIn(), ReceiverType.AllExceptMe));
                                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(),
                                        ReceiverType.AllExceptMe);
                                    Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(),
                                        ReceiverType.AllExceptMe);
                                }
                                ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                                DAOFactory.AccountDAO.WriteGeneralLog(session.Account.AccountId, session.CleanIpAddress,
                                    session.Character.CharacterId, GeneralLogType.Demotion, $"by: {Session.Character.Name}");
                            }
                            else
                            {
                                DAOFactory.AccountDAO.WriteGeneralLog(account.AccountId, "25.52.104.84", null,
                                    GeneralLogType.Demotion, $"by: {Session.Character.Name}");
                            }

                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                        }
                        else
                        {
                            Session.SendPacket(
                                Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_DO_THAT"), 10));
                        }
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
                catch
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(DemotePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $DropRate Command
        /// </summary>
        /// <param name="dropRatePacket"></param>
        public void DropRate(DropRatePacket dropRatePacket)
        {
            if (dropRatePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[DropRate]Value: {dropRatePacket.Value}");

                if (dropRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.Configuration.RateDrop = dropRatePacket.Value;
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(DropRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Effect Command
        /// </summary>
        /// <param name="effectCommandpacket"></param>
        public void Effect(EffectCommandPacket effectCommandpacket)
        {
            if (effectCommandpacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Effect]EffectId: {effectCommandpacket.EffectId}");

                Session.CurrentMapInstance?.Broadcast(
                    StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId,
                        effectCommandpacket.EffectId), Session.Character.PositionX, Session.Character.PositionY);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(EffectCommandPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Faction Command
        /// </summary>
        /// <param name="factionPacket"></param>
        public void Faction(FactionPacket factionPacket)
        {
            if (ServerManager.Instance.ChannelId == 51)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CHANGE_NOT_PERMITTED_ACT4"),
                        0));
                return;
            }
            if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4ShipAngel
                || Session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4ShipDemon)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CHANGE_NOT_PERMITTED_ACT4SHIP"),
                        0));
                return;
            }
            if (factionPacket != null)
            {
                Session.SendPacket("scr 0 0 0 0 0 0 0");
                if (Session.Character.Faction == FactionType.Angel)
                {
                    Session.Character.Faction = FactionType.Demon;
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_2"),
                            0));
                }
                else
                {
                    Session.Character.Faction = FactionType.Angel;
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey($"GET_PROTECTION_POWER_1"),
                            0));
                }
                Session.SendPacket(StaticPacketHelper.GenerateEff(UserType.Player,
                    Session.Character.CharacterId, 4799 + (byte)Session.Character.Faction));
                Session.SendPacket(Session.Character.GenerateFaction());
                if (ServerManager.Instance.ChannelId == 51)
                {
                    Session.SendPacket(Session.Character.GenerateFc());
                }
            }
        }

        /// <summary>
        /// $FairyXPRate Command
        /// </summary>
        /// <param name="fairyXpRatePacket"></param>
        public void FairyXpRate(FairyXpRatePacket fairyXpRatePacket)
        {
            if (fairyXpRatePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[FairyXPRate]Value: {fairyXpRatePacket.Value}");

                if (fairyXpRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.Configuration.RateFairyXP = fairyXpRatePacket.Value;
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("FAIRYXP_RATE_CHANGED"),
                            0));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(FairyXpRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $FamilyFaction Command
        /// </summary>
        /// <param name="familyFactionPacket"></param>
        public void FamilyFaction(FamilyFactionPacket familyFactionPacket)
        {
            if (ServerManager.Instance.ChannelId == 51)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CHANGE_NOT_PERMITTED_ACT4"),
                        0));
                return;
            }
            if (Session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4ShipAngel
                || Session.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4ShipDemon)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CHANGE_NOT_PERMITTED_ACT4SHIP"),
                        0));
                return;
            }
            if (familyFactionPacket != null)
            {
                if (String.IsNullOrEmpty(familyFactionPacket.FamilyName) && Session.Character.Family != null)
                {
                    Session.Character.Family.ChangeFaction(Session.Character.Family.FamilyFaction == 1 ? (byte)2 : (byte)1, Session);
                    return;
                }
                Family family = ServerManager.Instance.FamilyList.FirstOrDefault(s => s.Name == familyFactionPacket.FamilyName);
                if (family != null)
                {
                    family.ChangeFaction(family.FamilyFaction == 1 ? (byte)2 : (byte)1, Session);
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay("Family not found.", 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(FamilyFactionPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $GodMode Command
        /// </summary>
        /// <param name="godModePacket"></param>
        public void GodMode(GodModePacket godModePacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[GodMode]");

            Session.Character.HasGodMode = !Session.Character.HasGodMode;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        /// $Gogo Command
        /// </summary>
        /// <param name="gogoPacket"></param>
        public void Gogo(GogoPacket gogoPacket)
        {
            if (gogoPacket != null)
            {
                if (Session.Character.HasShopOpened || Session.Character.InExchangeOrTrade)
                {
                    Session.Character.DisposeShopAndExchange();
                }

                if (Session.Character.IsChangingMapInstance)
                {
                    return;
                }

                if (Session.CurrentMapInstance != null)
                {
                    Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                        $"[Gogo]MapId: {Session.CurrentMapInstance.Map.MapId} MapX: {gogoPacket.X} MapY: {gogoPacket.Y}");

                    if (gogoPacket.X == 0 && gogoPacket.Y == 0)
                    {
                        ServerManager.Instance.TeleportOnRandomPlaceInMap(Session, Session.CurrentMapInstance.MapInstanceId);
                    }
                    else
                    {
                        ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, Session.CurrentMapInstance.MapInstanceId, gogoPacket.X, gogoPacket.Y);
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GogoPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Gold Command
        /// </summary>
        /// <param name="goldPacket"></param>
        public void Gold(GoldPacket goldPacket)
        {
            if (goldPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Gold]Amount: {goldPacket.Amount}");

                long gold = goldPacket.Amount;
                long maxGold = ServerManager.Instance.Configuration.MaxGold;
                gold = gold > maxGold ? maxGold : gold;
                if (gold >= 0)
                {
                    Session.Character.Gold = gold;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_SET"),
                        0));
                    Session.SendPacket(Session.Character.GenerateGold());
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $BankGold Command
        /// </summary>
        /// <param name="bankGoldPacket"></param>
        public void BankGold(BankGoldPacket bankGoldPacket)
        {
            if (bankGoldPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[BankGold]Amount: {bankGoldPacket.Amount}");

                long bankGold = bankGoldPacket.Amount;
                long maxBankGold = ServerManager.Instance.Configuration.MaxGoldBank;
                bankGold = bankGold > maxBankGold ? maxBankGold : bankGold;

                if (maxBankGold >= 0)
                {
                    Session.Account.GoldBank = bankGold;
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BANKGOLD_SET"),
                        0));
                    Session.SendPacket(Session.Character.GenerateGold());
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_MUCH_GOLD"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $AddGold Command
        /// </summary>
        /// <param name="addGoldPacket"></param>
        public void AddGold(AddGoldPacket addGoldPacket)
        {
            if (addGoldPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[AddGold]Amount: {addGoldPacket.Amount}");

                long addgold = addGoldPacket.Amount;

                long maxGold = ServerManager.Instance.Configuration.MaxGold;

                if (Session.Character.Gold < maxGold)             
                {
                    if (addgold + Session.Character.Gold > maxGold)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_MUCH_GOLD"), 0));
                        return;
                    }

                    Session.Character.Gold += addgold;

                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_SET"),
                        0));
                    Session.SendPacket(Session.Character.GenerateGold());
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_MUCH_GOLD"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $AddBankGold Command
        /// </summary>
        /// <param name="addBankGoldPacket"></param>
        public void AddBankGold(AddBankGoldPacket addBankGoldPacket)
        {
            if (addBankGoldPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[AddbankGold]Amount: {addBankGoldPacket.Amount}");

                long addbankgold = addBankGoldPacket.Amount;

                long maxBankGold = ServerManager.Instance.Configuration.MaxGoldBank;

                if (addbankgold + Session.Account.GoldBank > maxBankGold)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_MUCH_GOLD"), 0));
                    return;
                }

                if (Session.Account.GoldBank < maxBankGold)
                {
                    
                    Session.Account.GoldBank += addbankgold;

                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BANKGOLD_SET"),0));
                    Session.SendPacket(Session.Character.GenerateGold());
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("TOO_MUCH_GOLD"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldPacket.ReturnHelp(), 10));
            }
        }


        /// <summary>
        /// $GoldDropRate Command
        /// </summary>
        /// <param name="goldDropRatePacket"></param>
        public void GoldDropRate(GoldDropRatePacket goldDropRatePacket)
        {
            if (goldDropRatePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[GoldDropRate]Value: {goldDropRatePacket.Value}");

                if (goldDropRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.Configuration.RateGoldDrop = goldDropRatePacket.Value;
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_DROP_RATE_CHANGED"),
                            0));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldDropRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $GoldRate Command
        /// </summary>
        /// <param name="goldRatePacket"></param>
        public void GoldRate(GoldRatePacket goldRatePacket)
        {
            if (goldRatePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[GoldRate]Value: {goldRatePacket.Value}");

                if (goldRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.Configuration.RateGold = goldRatePacket.Value;

                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("GOLD_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Guri Command
        /// </summary>
        /// <param name="guriCommandPacket"></param>
        public void Guri(GuriCommandPacket guriCommandPacket)
        {
            if (guriCommandPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Guri]Type: {guriCommandPacket.Type} Value: {guriCommandPacket.Value} Arguments: {guriCommandPacket.Argument}");

                Session.SendPacket(UserInterfaceHelper.GenerateGuri(guriCommandPacket.Type, guriCommandPacket.Argument,
                    Session.Character.CharacterId, guriCommandPacket.Value));
            }

            Session.Character.GenerateSay(GuriCommandPacket.ReturnHelp(), 10);
        }

        /// <summary>
        /// $HairColor Command
        /// </summary>
        /// <param name="hairColorPacket"></param>
        public void Haircolor(HairColorPacket hairColorPacket)
        {
            if (hairColorPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[HairColor]HairColor: {hairColorPacket.HairColor}");

                Session.Character.HairColor = hairColorPacket.HairColor;
                Session.SendPacket(Session.Character.GenerateEq());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(HairColorPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $HairStyle Command
        /// </summary>
        /// <param name="hairStylePacket"></param>
        public void Hairstyle(HairStylePacket hairStylePacket)
        {
            if (hairStylePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[HairStyle]HairStyle: {hairStylePacket.HairStyle}");

                Session.Character.HairStyle = hairStylePacket.HairStyle;
                Session.SendPacket(Session.Character.GenerateEq());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(HairStylePacket.ReturnHelp(), 10));
            }
        }

        public void HelpMe(HelpMePacket packet)
        {
            if (packet != null && !string.IsNullOrWhiteSpace(packet.Message))
            {
                int count = 0;
                foreach (ClientSession team in ServerManager.Instance.Sessions.Where(s =>
                    s.Account.Authority >= AuthorityType.TGS))
                {
                    if (team.HasSelectedCharacter)
                    {
                        count++;

                        // TODO: move that to resx soo we follow i18n
                        team.SendPacket(team.Character.GenerateSay($"User {Session.Character.Name} needs your help!",
                            12));
                        team.SendPacket(team.Character.GenerateSay($"Reason: {packet.Message}", 12));
                        team.SendPacket(
                            team.Character.GenerateSay("Please inform the family chat when you take care of!", 12));
                        team.SendPacket(Session.Character.GenerateSpk("Click this message to start chatting.", 5));
                        team.SendPacket(
                            UserInterfaceHelper.GenerateMsg($"User {Session.Character.Name} needs your help!", 0));
                    }
                }

                if (count != 0)
                {
                    Session.SendPacket(Session.Character.GenerateSay(
                        $"{count} Team members were informed! You should get a message shortly.", 10));
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(
                        "Sadly, there are no online team members right now. Please ask for help on our Discord Server at:",
                        10));
                    Session.SendPacket(Session.Character.GenerateSay("https://discord.gg/nosmoon", 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(HelpMePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $HeroXPRate Command
        /// </summary>
        /// <param name="heroXpRatePacket"></param>
        public void HeroXpRate(HeroXpRatePacket heroXpRatePacket)
        {
            if (heroXpRatePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[HeroXPRate]Value: {heroXpRatePacket.Value}");

                if (heroXpRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.Configuration.RateHeroicXP = heroXpRatePacket.Value;
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("HEROXP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(HeroXpRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Invisible Command
        /// </summary>
        /// <param name="invisiblePacket"></param>
        public void Invisible(InvisiblePacket invisiblePacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[Invisible]");

            Session.Character.Invisible = !Session.Character.Invisible;
            Session.Character.InvisibleGm = !Session.Character.InvisibleGm;
            Session.SendPacket(Session.Character.GenerateInvisible());
            Session.SendPacket(Session.Character.GenerateEq());
            Session.SendPacket(Session.Character.GenerateCMode());

            if (Session.Character.InvisibleGm)
            {
                Session.Character.Mates.Where(s => s.IsTeamMember).ToList()
                    .ForEach(s => Session.CurrentMapInstance?.Broadcast(s.GenerateOut()));
                Session.CurrentMapInstance?.Broadcast(Session,
                    StaticPacketHelper.Out(UserType.Player, Session.Character.CharacterId), ReceiverType.AllExceptMe);
            }
            else
            {
                foreach (Mate teamMate in Session.Character.Mates.Where(m => m.IsTeamMember))
                {
                    teamMate.PositionX = Session.Character.PositionX;
                    teamMate.PositionY = Session.Character.PositionY;
                    Parallel.ForEach(Session.CurrentMapInstance.Sessions.Where(s => s.Character != null), s =>
                    {
                        if (ServerManager.Instance.ChannelId != 51 || Session.Character.Faction == s.Character.Faction)
                        {
                            s.SendPacket(teamMate.GenerateIn(false, ServerManager.Instance.ChannelId == 51));
                        }
                        else
                        {
                            s.SendPacket(teamMate.GenerateIn(true, ServerManager.Instance.ChannelId == 51, s.Account.Authority));
                        }
                    });
                    Session.SendPacket(Session.Character.GeneratePinit());
                    Session.Character.Mates.ForEach(s => Session.SendPacket(s.GenerateScPacket()));
                    Session.SendPackets(Session.Character.GeneratePst());
                }
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateIn(),
                    ReceiverType.AllExceptMe);
                Session.CurrentMapInstance?.Broadcast(Session, Session.Character.GenerateGidx(),
                    ReceiverType.AllExceptMe);
            }
        }

        /// <summary>
        /// $ItemRain Command
        /// </summary>
        /// <param name="itemRainPacket"></param>
        public void ItemRain(ItemRainPacket itemRainPacket)
        {
            if (itemRainPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                       $"[ItemRain]ItemVNum: {itemRainPacket.VNum} Amount: {itemRainPacket.Amount} Count: {itemRainPacket.Count} Time: {itemRainPacket.Time}");

                short vnum = itemRainPacket.VNum;
                short amount = itemRainPacket.Amount;
                if (amount > 9999) { amount = 9999; }
                int count = itemRainPacket.Count;
                int time = itemRainPacket.Time;

                GameObject.MapInstance instance = Session.CurrentMapInstance;

                Observable.Timer(TimeSpan.FromSeconds(0)).SafeSubscribe(observer =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        MapCell cell = instance.Map.GetRandomPosition();
                        MonsterMapItem droppedItem = new MonsterMapItem(cell.X, cell.Y, vnum, amount);
                        instance.DroppedList[droppedItem.TransportId] = droppedItem;
                        instance.Broadcast(
                            $"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {(droppedItem.GoldAmount > 1 ? droppedItem.GoldAmount : droppedItem.Amount)} 0 -1");

                        System.Threading.Thread.Sleep(time * 1000 / count);
                    }
                });
            }
        }

        /// <summary>
        /// $Kick Command
        /// </summary>
        /// <param name="kickPacket"></param>
        public void Kick(KickPacket kickPacket)
        {
            if (kickPacket != null)
            {
                if (kickPacket.CharacterName == "*")
                {
                    Parallel.ForEach(ServerManager.Instance.Sessions, session => session.Disconnect());
                }

                ServerManager.Instance.Kick(kickPacket.CharacterName);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(KickPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $KickSession Command
        /// </summary>
        /// <param name="kickSessionPacket"></param>
        public void KickSession(KickSessionPacket kickSessionPacket)
        {
            if (kickSessionPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Kick]AccountName: {kickSessionPacket.AccountName} SessionId: {kickSessionPacket.SessionId}");

                if (kickSessionPacket.SessionId.HasValue) //if you set the sessionId, remove account verification
                {
                    kickSessionPacket.AccountName = "";
                }

                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                AccountDTO account = DAOFactory.AccountDAO.LoadByName(kickSessionPacket.AccountName);
                CommunicationServiceClient.Instance.KickSession(account?.AccountId, kickSessionPacket.SessionId);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(KickSessionPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Kill Command
        /// </summary>
        /// <param name="killPacket"></param>
        public void Kill(KillPacket killPacket)
        {
            if (killPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Kill]CharacterName: {killPacket.CharacterName}");

                ClientSession sess = ServerManager.Instance.GetSessionByCharacterName(killPacket.CharacterName);
                if (sess != null)
                {
                    if (sess.Character.Authority >= AuthorityType.Founder)
                    {
                        Session.SendPacket("info Succesfully demoted yourself.");
                        return;
                    }

                    if (sess.Character.HasGodMode)
                    {
                        return;
                    }

                    if (sess.Character.Hp < 1)
                    {
                        return;
                    }

                    sess.Character.Hp = 0;
                    sess.Character.LastDefence = DateTime.Now;
                    Session.CurrentMapInstance?.Broadcast(StaticPacketHelper.SkillUsed(UserType.Player,
                        Session.Character.CharacterId, 1, sess.Character.CharacterId, 1114, 4, 11, 4260, 0, 0, false, 0, 60000, 3, 0));
                    sess.SendPacket(sess.Character.GenerateStat());
                    if (sess.Character.IsVehicled)
                    {
                        sess.Character.RemoveVehicle();
                    }
                    ServerManager.Instance.AskRevive(sess.Character.CharacterId);
                    Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(KillPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $LeaveAct4 Command
        /// </summary>
        /// <param name="leaveAct4Packet"></param>
        public void LeaveAct4(LeaveAct4Packet leaveAct4Packet)
        {
            if (leaveAct4Packet != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[LeaveAct4]");

                if (Session.Character.Channel.ChannelId == 51)
                {
                    string connection = CommunicationServiceClient.Instance.RetrieveOriginWorld(Session.Character.AccountId);
                    if (string.IsNullOrWhiteSpace(connection))
                    {
                        return;
                    }
                    Session.Character.MapId = 145;
                    Session.Character.MapX = 51;
                    Session.Character.MapY = 41;
                    int port = Convert.ToInt32(connection.Split(':')[1]);
                    Session.Character.ChangeChannel(connection.Split(':')[0], port, 3);
                }
            }

            Session.Character.GenerateSay(LeaveAct4Packet.ReturnHelp(), 10);
        }

        /// <summary>
        /// $PenaltyLog Command
        /// </summary>
        /// <param name="penaltyLogPacket"></param>
        public void ListPenalties(PenaltyLogPacket penaltyLogPacket)
        {
            string returnHelp = CharacterStatsPacket.ReturnHelp();
            if (penaltyLogPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[PenaltyLog]CharacterName: {penaltyLogPacket.CharacterName}");

                string name = penaltyLogPacket.CharacterName;
                if (!string.IsNullOrEmpty(name))
                {
                    CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(name);
                    if (character != null)
                    {
                        bool separatorSent = false;

                        void WritePenalty(PenaltyLogDTO penalty)
                        {
                            Session.SendPacket(Session.Character.GenerateSay($"Type: {penalty.Penalty}", 13));
                            Session.SendPacket(Session.Character.GenerateSay($"AdminName: {penalty.AdminName}", 13));
                            Session.SendPacket(Session.Character.GenerateSay($"Reason: {penalty.Reason}", 13));
                            Session.SendPacket(Session.Character.GenerateSay($"DateStart: {penalty.DateStart}", 13));
                            Session.SendPacket(Session.Character.GenerateSay($"DateEnd: {penalty.DateEnd}", 13));
                            Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
                            separatorSent = true;
                        }

                        IEnumerable<PenaltyLogDTO> penaltyLogs = ServerManager.Instance.PenaltyLogs
                            .Where(s => s.AccountId == character.AccountId).ToList();

                        //PenaltyLogDTO penalty = penaltyLogs.LastOrDefault(s => s.DateEnd > DateTime.Now);
                        Session.SendPacket(Session.Character.GenerateSay("----- PENALTIES -----", 13));

                        #region Warnings

                        Session.SendPacket(Session.Character.GenerateSay("----- WARNINGS -----", 13));
                        foreach (PenaltyLogDTO penaltyLog in penaltyLogs.Where(s => s.Penalty == PenaltyType.Warning)
                            .OrderBy(s => s.DateStart))
                        {
                            WritePenalty(penaltyLog);
                        }

                        if (!separatorSent)
                        {
                            Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
                        }

                        separatorSent = false;

                        #endregion

                        #region Mutes

                        Session.SendPacket(Session.Character.GenerateSay("----- MUTES -----", 13));
                        foreach (PenaltyLogDTO penaltyLog in penaltyLogs.Where(s => s.Penalty == PenaltyType.Muted)
                            .OrderBy(s => s.DateStart))
                        {
                            WritePenalty(penaltyLog);
                        }

                        if (!separatorSent)
                        {
                            Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
                        }

                        separatorSent = false;

                        #endregion

                        #region Bans

                        Session.SendPacket(Session.Character.GenerateSay("----- BANS -----", 13));
                        foreach (PenaltyLogDTO penaltyLog in penaltyLogs.Where(s => s.Penalty == PenaltyType.Banned)
                            .OrderBy(s => s.DateStart))
                        {
                            WritePenalty(penaltyLog);
                        }

                        if (!separatorSent)
                        {
                            Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
                        }

                        #endregion

                        Session.SendPacket(Session.Character.GenerateSay("----- SUMMARY -----", 13));
                        Session.SendPacket(Session.Character.GenerateSay(
                            $"Warnings: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Warning)}", 13));
                        Session.SendPacket(
                            Session.Character.GenerateSay(
                                $"Mutes: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Muted)}", 13));
                        Session.SendPacket(
                            Session.Character.GenerateSay(
                                $"Bans: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Banned)}", 13));
                        Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(Session.Character.GenerateSay(returnHelp, 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(returnHelp, 10));
            }
        }

        /// <summary>
        /// $MapDance Command
        /// </summary>
        /// <param name="mapDancePacket"></param>
        public void MapDance(MapDancePacket mapDancePacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[MapDance]");

            if (Session.HasCurrentMapInstance)
            {
                Session.CurrentMapInstance.IsDancing = !Session.CurrentMapInstance.IsDancing;
                if (Session.CurrentMapInstance.IsDancing)
                {
                    Session.Character.Dance();
                    Session.CurrentMapInstance?.Broadcast("dance 2");
                }
                else
                {
                    Session.Character.Dance();
                    Session.CurrentMapInstance?.Broadcast("dance");
                }

                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
        }

        /// <summary>
        /// $MapPVP Command
        /// </summary>
        /// <param name="mapPvpPacket"></param>
        public void MapPvp(MapPVPPacket mapPvpPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[MapPVP]");

            Session.CurrentMapInstance.IsPVP = !Session.CurrentMapInstance.IsPVP;
            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        /// <summary>
        /// $MapReset Command
        /// </summary>
        /// <param name="mapResetPacket"></param>
        public void MapReset(MapResetPacket mapResetPacket)
        {
            if (mapResetPacket != null)
            {
                if (Session.Character.IsChangingMapInstance)
                {
                    return;
                }
                if (Session.CurrentMapInstance != null)
                {
                    Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                        $"[MapReset]MapId: {Session.CurrentMapInstance.Map.MapId}");

                    GameObject.MapInstance newMapInstance = ServerManager.ResetMapInstance(Session.CurrentMapInstance);

                    Parallel.ForEach(Session.CurrentMapInstance.Sessions, sess =>
                    ServerManager.Instance.ChangeMapInstance(sess.Character.CharacterId, newMapInstance.MapInstanceId, sess.Character.PositionX, sess.Character.PositionY));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MapResetPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $MapStat Command
        /// </summary>
        /// <param name="mapStatPacket"></param>
        public void MapStats(MapStatisticsPacket mapStatPacket)
        {
            // lower the boilerplate
            void SendMapStats(MapDTO map, GameObject.MapInstance mapInstance)
            {
                if (map != null && mapInstance != null)
                {
                    Session.SendPacket(Session.Character.GenerateSay("-------------MapData-------------", 10));
                    Session.SendPacket(Session.Character.GenerateSay(
                        $"MapId: {map.MapId}\n" +
                        $"MapMusic: {map.Music}\n" +
                        $"MapName: {map.Name}\n" +
                        $"MapShopAllowed: {map.ShopAllowed}", 10));
                    Session.SendPacket(Session.Character.GenerateSay("---------------------------------", 10));
                    Session.SendPacket(Session.Character.GenerateSay("---------MapInstanceData---------", 10));
                    Session.SendPacket(Session.Character.GenerateSay(
                        $"MapInstanceId: {mapInstance.MapInstanceId}\n" +
                        $"MapInstanceType: {mapInstance.MapInstanceType}\n" +
                        $"MapMonsterCount: {mapInstance.Monsters.Count}\n" +
                        $"MapNpcCount: {mapInstance.Npcs.Count}\n" +
                        $"MapPortalsCount: {mapInstance.Portals.Count}\n" +
                        $"MapInstanceUserShopCount: {mapInstance.UserShops.Count}\n" +
                        $"SessionCount: {mapInstance.Sessions.Count()}\n" +
                        $"MapInstanceXpRate: {mapInstance.XpRate}\n" +
                        $"MapInstanceDropRate: {mapInstance.DropRate}\n" +
                        $"MapInstanceMusic: {mapInstance.InstanceMusic}\n" +
                        $"ShopsAllowed: {mapInstance.ShopAllowed}\n" +
                        $"DropAllowed: {mapInstance.DropAllowed}\n" +
                        $"IsPVP: {mapInstance.IsPVP}\n" +
                        $"IsSleeping: {mapInstance.IsSleeping}\n" +
                        $"Dance: {mapInstance.IsDancing}", 10));
                    Session.SendPacket(Session.Character.GenerateSay("---------------------------------", 10));
                }
            }

            if (mapStatPacket != null)
            {
                if (mapStatPacket.MapId.HasValue)
                {
                    MapDTO map = DAOFactory.MapDAO.LoadById(mapStatPacket.MapId.Value);
                    GameObject.MapInstance mapInstance = ServerManager.GetMapInstanceByMapId(mapStatPacket.MapId.Value);
                    if (map != null && mapInstance != null)
                    {
                        SendMapStats(map, mapInstance);
                    }
                }
                else if (Session.HasCurrentMapInstance)
                {
                    MapDTO map = DAOFactory.MapDAO.LoadById(Session.CurrentMapInstance.Map.MapId);
                    if (map != null)
                    {
                        SendMapStats(map, Session.CurrentMapInstance);
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MapStatisticsPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Miniland Command
        /// </summary>
        /// <param name="minilandPacket"></param>
        public void Miniland(MinilandPacket minilandPacket)
        {
            if (minilandPacket != null)
            {
                if (string.IsNullOrEmpty(minilandPacket.CharacterName))
                {
                    Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Miniland]");

                    ServerManager.Instance.JoinMiniland(Session, Session);
                }
                else
                {
                    ClientSession session = ServerManager.Instance.GetSessionByCharacterName(minilandPacket.CharacterName);
                    if (session != null)
                    {
                        Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Miniland]CharacterName: {minilandPacket.CharacterName}");

                        ServerManager.Instance.JoinMiniland(Session, session);
                    }
                }
            }

            Session.Character.GenerateSay(MinilandPacket.ReturnHelp(), 10);
        }

        /// <summary>
        /// $Mob Command
        /// </summary>
        /// <param name="mobPacket"></param>
        public void Mob(MobPacket mobPacket)
        {
            if (mobPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Mob]NpcMonsterVNum: {mobPacket.NpcMonsterVNum} Amount: {mobPacket.Amount} IsMoving: {mobPacket.IsMoving}");

                if (Session.HasCurrentMapInstance && Session.HasCurrentMapInstance)
                {
                    NpcMonster npcmonster = ServerManager.GetNpcMonster(mobPacket.NpcMonsterVNum);
                    if (npcmonster == null)
                    {
                        return;
                    }

                    Random random = new Random();
                    for (int i = 0; i < mobPacket.Amount; i++)
                    {
                        List<MapCell> possibilities = new List<MapCell>();
                        for (short x = -4; x < 5; x++)
                        {
                            for (short y = -4; y < 5; y++)
                            {
                                possibilities.Add(new MapCell { X = x, Y = y });
                            }
                        }

                        foreach (MapCell possibilitie in possibilities.OrderBy(s => random.Next()))
                        {
                            short mapx = (short)(Session.Character.PositionX + possibilitie.X);
                            short mapy = (short)(Session.Character.PositionY + possibilitie.Y);
                            if (!Session.CurrentMapInstance?.Map.IsBlockedZone(mapx, mapy) ?? false)
                            {
                                break;
                            }
                        }

                        if (Session.CurrentMapInstance != null)
                        {
                            MapMonster monster = new MapMonster
                            {
                                MonsterVNum = mobPacket.NpcMonsterVNum,
                                MapY = Session.Character.PositionY,
                                MapX = Session.Character.PositionX,
                                MapId = Session.Character.MapInstance.Map.MapId,
                                Position = Session.Character.Direction,
                                IsMoving = mobPacket.IsMoving,
                                MapMonsterId = Session.CurrentMapInstance.GetNextMonsterId(),
                                ShouldRespawn = false
                            };
                            monster.Initialize(Session.CurrentMapInstance);
                            Session.CurrentMapInstance.AddMonster(monster);
                            Session.CurrentMapInstance.Broadcast(monster.GenerateIn());
                        }
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MobPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Morph Command
        /// </summary>
        /// <param name="morphPacket"></param>
        public void Morph(MorphPacket morphPacket)
        {
            if (morphPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Morph]MorphId: {morphPacket.MorphId} MorphDesign: {morphPacket.MorphDesign} Upgrade: {morphPacket.Upgrade} MorphId: {morphPacket.ArenaWinner}");

                if (morphPacket.MorphId < 30 && morphPacket.MorphId > 0)
                {
                    Session.Character.UseSp = true;
                    Session.Character.Morph = morphPacket.MorphId;
                    Session.Character.MorphUpgrade = morphPacket.Upgrade;
                    Session.Character.MorphUpgrade2 = morphPacket.MorphDesign;
                    Session.Character.ArenaWinner = morphPacket.ArenaWinner;
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                }
                else if (morphPacket.MorphId > 30)
                {
                    Session.Character.IsVehicled = true;
                    Session.Character.Morph = morphPacket.MorphId;
                    Session.Character.ArenaWinner = morphPacket.ArenaWinner;
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                }
                else
                {
                    Session.Character.IsVehicled = false;
                    Session.Character.UseSp = false;
                    Session.Character.ArenaWinner = 0;
                    Session.SendPacket(Session.Character.GenerateCond());
                    Session.SendPacket(Session.Character.GenerateLev());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateCMode());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MorphPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Mute Command
        /// </summary>
        /// <param name="mutePacket"></param>
        public void Mute(MutePacket mutePacket)
        {
            if (mutePacket != null)
            {

                if (mutePacket.Duration == 0)
                {
                    mutePacket.Duration = 60;
                }

                mutePacket.Reason = mutePacket.Reason?.Trim();
                MuteMethod(mutePacket.CharacterName, mutePacket.Reason, mutePacket.Duration);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MutePacket.ReturnHelp(), 10));
            }
        }

        public void Time(Time t)
        {
            var time = DateTime.Now;
            string output = "---------------Server Time---------------\n";
            output += $"[{time.ToString()}]\n";
            output += "---------------Server Time---------------\n";
            Session.SendPacket(Session.Character.GenerateSay(output, 11));
        }

        /// <summary>
        /// $SNPC Command
        /// </summary>
        /// <param name="NpcPacket"></param>
        public void Npc(NPCPacket NpcPacket)
        {
            if (NpcPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[NPC]NpcMonsterVNum: {NpcPacket.NpcMonsterVNum} Amount: {NpcPacket.Amount} IsMoving: {NpcPacket.IsMoving}");

                if (Session.HasCurrentMapInstance && Session.HasCurrentMapInstance)
                {
                    NpcMonster npcmonster = ServerManager.GetNpcMonster(NpcPacket.NpcMonsterVNum);
                    if (npcmonster == null)
                    {
                        return;
                    }

                    Random random = new Random();
                    for (int i = 0; i < NpcPacket.Amount; i++)
                    {
                        List<MapCell> possibilities = new List<MapCell>();
                        for (short x = -4; x < 5; x++)
                        {
                            for (short y = -4; y < 5; y++)
                            {
                                possibilities.Add(new MapCell { X = x, Y = y });
                            }
                        }

                        foreach (MapCell possibilitie in possibilities.OrderBy(s => random.Next()))
                        {
                            short mapx = (short)(Session.Character.PositionX + possibilitie.X);
                            short mapy = (short)(Session.Character.PositionY + possibilitie.Y);
                            if (!Session.CurrentMapInstance?.Map.IsBlockedZone(mapx, mapy) ?? false)
                            {
                                break;
                            }
                        }

                        if (Session.CurrentMapInstance != null)
                        {
                            MapNpc npc = new MapNpc
                            {
                                NpcVNum = NpcPacket.NpcMonsterVNum,
                                MapY = Session.Character.PositionY,
                                MapX = Session.Character.PositionX,
                                MapId = Session.Character.MapInstance.Map.MapId,
                                Position = Session.Character.Direction,
                                IsMoving = NpcPacket.IsMoving,
                                ShouldRespawn = false,
                                MapNpcId = Session.CurrentMapInstance.GetNextNpcId()
                            };
                            npc.Initialize(Session.CurrentMapInstance);
                            Session.CurrentMapInstance.AddNPC(npc);
                            Session.CurrentMapInstance.Broadcast(npc.GenerateIn());
                        }
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(NPCPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Packet Command
        /// </summary>
        /// <param name="packetCallbackPacket"></param>
        public void PacketCallBack(PacketCallbackPacket packetCallbackPacket)
        {
            if (packetCallbackPacket != null)
            {
                Session.SendPacket(packetCallbackPacket.Packet);
                Session.SendPacket(Session.Character.GenerateSay(packetCallbackPacket.Packet, 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(PacketCallbackPacket.ReturnHelp(), 10));
            }
        }

        public void PartnerSpXp(PartnerSpXpPacket partnerSpXpPacket)
        {
            if (partnerSpXpPacket == null)
            {
                return;
            }

            Mate mate = Session.Character.Mates?.ToList().FirstOrDefault(s => s.IsTeamMember && s.MateType == MateType.Partner);

            if (mate?.Sp != null)
            {
                mate.Sp.FullXp();
                Session.SendPacket(mate.GenerateScPacket());
            }
        }

        /// <summary>
        /// $Maintenance Command
        /// </summary>
        /// <param name="maintenancePacket"></param>
        public void PlanMaintenance(MaintenancePacket maintenancePacket)
        {
            if (maintenancePacket != null)
            {

                DateTime dateStart = DateTime.Now.AddMinutes(maintenancePacket.Delay);
                MaintenanceLogDTO maintenance = new MaintenanceLogDTO
                {
                    DateEnd = dateStart.AddMinutes(maintenancePacket.Duration),
                    DateStart = dateStart,
                    Reason = maintenancePacket.Reason
                };
                DAOFactory.MaintenanceLogDAO.Insert(maintenance);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(MaintenancePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $PortalTo Command
        /// </summary>
        /// <param name="portalToPacket"></param>
        public void PortalTo(PortalToPacket portalToPacket)
        {
            if (portalToPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[PortalTo]DestinationMapId: {portalToPacket.DestinationMapId} DestinationMapX: {portalToPacket.DestinationX} DestinationY: {portalToPacket.DestinationY}");

                AddPortal(portalToPacket.DestinationMapId, portalToPacket.DestinationX, portalToPacket.DestinationY,
                    portalToPacket.PortalType == null ? (short)-1 : (short)portalToPacket.PortalType, false);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(PortalToPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Position Command
        /// </summary>
        /// <param name="positionPacket"></param>
        public void Position(PositionPacket positionPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[Position]");

            Session.SendPacket(Session.Character.GenerateSay(
                $"Map:{Session.Character.MapInstance.Map.MapId} - X:{Session.Character.PositionX} - Y:{Session.Character.PositionY} - Dir:{Session.Character.Direction} - Cell:{Session.CurrentMapInstance.Map.Tiles[Session.Character.PositionX, Session.Character.PositionY].Value}",
                12));
        }

        /// <summary>
        /// $Rarify Command
        /// </summary>
        /// <param name="rarifyPacket"></param>
        public void Rarify(RarifyPacket rarifyPacket)
        {
            if (rarifyPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Rarify]Slot: {rarifyPacket.Slot} Mode: {rarifyPacket.Mode} Protection: {rarifyPacket.Protection}");

                if (rarifyPacket.Slot >= 0)
                {
                    ItemInstance wearableInstance = Session.Character.Inventory.LoadBySlotAndType(rarifyPacket.Slot, 0);
                    wearableInstance?.RarifyItem(Session, rarifyPacket.Mode, rarifyPacket.Protection);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(RarifyPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ReloadSI Command
        /// </summary>
        /// <param name="reloadSIPacket"></param>
        public void ReloadSI(ReloadSIPacket reloadSIPacket)
        {
            if (reloadSIPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[ReloadSI]");

                ServerManager.Instance.LoadScriptedInstances();
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ReloadSIPacket.ReturnHelp(), 10));
            }
        }

        public void RemoveDupitem(RemoveDupItemPacket e)
        {
            //Stupid Harcod
            var item = DAOFactory.ItemInstanceDAO.LoadAll().OrderBy(s => s.EquipmentSerialId);
            ItemInstanceDTO itemsaved = null;
            var count = 0;
            foreach (var ii in item)
            {
                if (itemsaved == null)
                {
                    itemsaved = ii;
                    continue;
                }
                count++;
                if (itemsaved.EquipmentSerialId == ii.EquipmentSerialId)
                {
                    var items = DAOFactory.ItemInstanceDAO.LoadAll().Where(s => s.EquipmentSerialId == ii.EquipmentSerialId);
                    foreach (var iii in items)
                    {
                        Console.WriteLine("that item " + iii.EquipmentSerialId);
                        DAOFactory.MinilandObjectDAO.DeleteByItemId(iii.EquipmentSerialId);
                        DAOFactory.BazaarItemDAO.DeleteByItemId(iii.EquipmentSerialId);
                        DAOFactory.ItemInstanceDAO.Delete(iii.Id);
                    }
                    continue;
                }
                itemsaved = null;
                Console.WriteLine(count);
            }
        }

        /// <summary>
        /// $RemovePortal Command
        /// </summary>
        /// <param name="removePortalPacket"></param>
        public void RemovePortal(RemovePortalPacket removePortalPacket)
        {
            if (Session.HasCurrentMapInstance)
            {
                Portal portal = Session.CurrentMapInstance.Portals.Find(s =>
                    s.SourceMapInstanceId == Session.Character.MapInstanceId && Map.GetDistance(
                        new MapCell { X = s.SourceX, Y = s.SourceY },
                        new MapCell { X = Session.Character.PositionX, Y = Session.Character.PositionY }) < 10);
                if (portal != null)
                {
                    Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                        $"[RemovePortal]MapId: {portal.SourceMapId} MapX: {portal.SourceX} MapY: {portal.SourceY}");

                    Session.SendPacket(Session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("NEAREST_PORTAL"), portal.SourceMapId,
                            portal.SourceX, portal.SourceY), 12));
                    portal.IsDisabled = true;
                    Session.CurrentMapInstance?.Broadcast(portal.GenerateGp());
                    Session.CurrentMapInstance.Portals.Remove(portal);
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NO_PORTAL_FOUND"), 11));
                }
            }
        }

        // if (Amount <= 0) { Amount = 1; }
        /// <summary>
        /// $ReputationRate Command
        /// </summary>
        /// <param name="reputationRatePacket"></param>
        public void ReputationRate(ReputationRatePacket reputationRatePacket)
        {
            if (reputationRatePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[ReputationRate]Value: {reputationRatePacket.Value}");

                if (reputationRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.Configuration.RateReputation = reputationRatePacket.Value;

                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("REPUTATION_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GoldRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Resize Command
        /// </summary>
        /// <param name="resizePacket"></param>
        public void Resize(ResizePacket resizePacket)
        {
            if (resizePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Resize]Size: {resizePacket.Value}");

                if (resizePacket.Value >= 0)
                {
                    Session.Character.Size = resizePacket.Value;
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateScal());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ResizePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Restart Command
        /// </summary>
        /// <param name="restartPacket"></param>
        public void Restart(RestartPacket restartPacket)
        {
            int time = restartPacket.Time > 0 ? restartPacket.Time : 5;

            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Restart]Time: {time}");

            if (ServerManager.Instance.TaskShutdown != null)
            {
                ServerManager.Instance.ShutdownStop = true;
                ServerManager.Instance.TaskShutdown = null;
            }
            else
            {
                ServerManager.Instance.IsReboot = true;
                ServerManager.Instance.TaskShutdown = ServerManager.Instance.ShutdownTaskAsync(time);
            }

            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
        }

        // int distance = Map.GetDistance(new MapCell { X = Session.Character.PositionX, Y =
        // Session.Character.PositionY }, new MapCell { X = npc.MapX, Y = npc.MapY }); if (distance >
        // 5) { Session.SendPacket(Session.Character.GenerateSay(
        // string.Format(Language.Instance.GetMessageFromKey("TOO_FAR")), 11)); return; }
        /// <summary>
        /// $RestartAll Command
        /// </summary>
        /// <param name="restartAllPacket"></param>
        public void RestartAll(RestartAllPacket restartAllPacket)
        {
            if (restartAllPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[RestartAll]");

                string worldGroup = !string.IsNullOrEmpty(restartAllPacket.WorldGroup) ? restartAllPacket.WorldGroup : ServerManager.Instance.ServerGroup;

                int time = restartAllPacket.Time;

                if (time < 1)
                {
                    time = 5;
                }

                CommunicationServiceClient.Instance.Restart(worldGroup, time);

                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(RestartAllPacket.ReturnHelp(), 10));
            }
        }

        // if (monster.IsAlive) {
        // Session.CurrentMapInstance.Broadcast(StaticPacketHelper.Out(UserType.Monster,
        // monster.MapMonsterId)); Session.SendPacket(Session.Character.GenerateSay(
        // string.Format(Language.Instance.GetMessageFromKey("MONSTER_REMOVED"),
        // monster.MapMonsterId, monster.Monster.Name, monster.MapId, monster.MapX, monster.MapY),
        // 12)); Session.CurrentMapInstance.RemoveMonster(monster);
        // Session.CurrentMapInstance.RemovedMobNpcList.Add(monster); if
        // (DAOFactory.MapMonsterDAO.LoadById(monster.MapMonsterId) != null) {
        // DAOFactory.MapMonsterDAO.DeleteById(monster.MapMonsterId); } } else {
        // Session.SendPacket(Session.Character.GenerateSay(
        // string.Format(Language.Instance.GetMessageFromKey("MONSTER_NOT_ALIVE")), 11)); } } else
        // if (npc != null) {
        /// <summary>
        /// $SearchItem Command
        /// </summary>
        /// <param name="searchItemPacket"></param>
        public void SearchItem(SearchItemPacket searchItemPacket)
        {
            if (searchItemPacket != null)
            {
                string contents = searchItemPacket.Contents;
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[SearchItem]Contents: {(string.IsNullOrEmpty(contents) ? "none" : contents)}");

                string name = "";
                byte page = 0;
                if (!string.IsNullOrEmpty(contents))
                {
                    string[] packetsplit = contents.Split(' ');
                    bool withPage = byte.TryParse(packetsplit[0], out page);
                    name = packetsplit.Length == 1 && withPage
                        ? ""
                        : packetsplit.Skip(withPage ? 1 : 0).Aggregate((a, b) => a + ' ' + b);
                }

                IEnumerable<ItemDTO> itemlist = DAOFactory.ItemDAO.FindByName(name).OrderBy(s => s.VNum)
                    .Skip(page * 200).Take(200).ToList();
                if (itemlist.Any())
                {
                    foreach (ItemDTO item in itemlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay(
                            $"[SearchItem:{page}]Item: {(string.IsNullOrEmpty(item.Name) ? "none" : item.Name)} VNum: {item.VNum}",
                            12));
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("ITEM_NOT_FOUND"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SearchItemPacket.ReturnHelp(), 10));
            }
        }

        // int distance = Map.GetDistance(new MapCell { X = Session.Character.PositionX, Y =
        // Session.Character.PositionY }, new MapCell { X = monster.MapX, Y = monster.MapY }); if
        // (distance > 5) { Session.SendPacket(Session.Character.GenerateSay(
        // string.Format(Language.Instance.GetMessageFromKey("TOO_FAR")), 11)); return; }
        /// <summary>
        /// $SearchMonster Command
        /// </summary>
        /// <param name="searchMonsterPacket"></param>
        public void SearchMonster(SearchMonsterPacket searchMonsterPacket)
        {
            if (searchMonsterPacket != null)
            {
                string contents = searchMonsterPacket.Contents;
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[SearchMonster]Contents: {(string.IsNullOrEmpty(contents) ? "none" : contents)}");

                string name = "";
                byte page = 0;
                if (!string.IsNullOrEmpty(contents))
                {
                    string[] packetsplit = contents.Split(' ');
                    bool withPage = byte.TryParse(packetsplit[0], out page);
                    name = packetsplit.Length == 1 && withPage
                        ? ""
                        : packetsplit.Skip(withPage ? 1 : 0).Aggregate((a, b) => a + ' ' + b);
                }

                IEnumerable<NpcMonsterDTO> monsterlist = DAOFactory.NpcMonsterDAO.FindByName(name)
                    .OrderBy(s => s.NpcMonsterVNum).Skip(page * 200).Take(200).ToList();
                if (monsterlist.Any())
                {
                    foreach (NpcMonsterDTO npcMonster in monsterlist)
                    {
                        Session.SendPacket(Session.Character.GenerateSay(
                            $"[SearchMonster:{page}]Monster: {(string.IsNullOrEmpty(npcMonster.Name) ? "none" : npcMonster.Name)} VNum: {npcMonster.NpcMonsterVNum}",
                            12));
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MONSTER_NOT_FOUND"), 11));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SearchMonsterPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ServerInfo Command
        /// </summary>
        /// <param name="serverInfoPacket"></param>
        public void ServerInfo(ServerInfoPacket serverInfoPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), "[ServerInfo]");

            Session.SendPacket(Session.Character.GenerateSay($"------------Server Info------------", 11));

            long ActualChannelId = 0;

            CommunicationServiceClient.Instance.GetOnlineCharacters().Where(s => serverInfoPacket.ChannelId == null || s[1] == serverInfoPacket.ChannelId).OrderBy(s => s[1]).ToList().ForEach(s =>
            {
                if (s[1] > ActualChannelId)
                {
                    if (ActualChannelId > 0)
                    {
                        Session.SendPacket(Session.Character.GenerateSay("----------------------------------------", 11));
                    }
                    ActualChannelId = s[1];
                    Session.SendPacket(Session.Character.GenerateSay($"-------------Channel:{ActualChannelId}-------------", 11));
                }
                CharacterDTO Character = DAOFactory.CharacterDAO.LoadById(s[0]);
                Session.SendPacket(
                    Session.Character.GenerateSay(
                        $"CharacterName: {Character.Name} | CharacterId: {Character.CharacterId} | SessionId: {s[2]}", 12));
            });

            Session.SendPacket(Session.Character.GenerateSay("----------------------------------------", 11));
        }

        public void KickChannel(KickChannelPacket packet)
        {
            if (packet.ChannelId == ServerManager.Instance.ChannelId)
            {
                Parallel.ForEach(ServerManager.Instance.Sessions, session => session.Disconnect());
                return;
            }

            if (packet.ChannelId == 0)
            {
                return;
            }

            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = null,
                IsItemSpeaker = false,
                Message = packet.ChannelId.ToString(),
                SourceCharacterId = Session.Character.CharacterId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Type = MessageType.KickChannel
            });
        }

//
        /// <summary>
        /// $SetPerfection Command
        /// </summary>
        /// <param name="setPerfectionPacket"></param>
        public void SetPerfection(SetPerfectionPacket setPerfectionPacket)
        {
            if (setPerfectionPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[SetPerfection]Slot: {setPerfectionPacket.Slot} Type: {setPerfectionPacket.Type} Value: {setPerfectionPacket.Value}");

                if (setPerfectionPacket.Slot >= 0)
                {
                    ItemInstance specialistInstance =
                        Session.Character.Inventory.LoadBySlotAndType(setPerfectionPacket.Slot, 0);

                    if (specialistInstance != null)
                    {
                        switch (setPerfectionPacket.Type)
                        {
                            case 0:
                                specialistInstance.SpStoneUpgrade = setPerfectionPacket.Value;
                                break;

                            case 1:
                                specialistInstance.SpDamage = setPerfectionPacket.Value;
                                break;

                            case 2:
                                specialistInstance.SpDefence = setPerfectionPacket.Value;
                                break;

                            case 3:
                                specialistInstance.SpElement = setPerfectionPacket.Value;
                                break;

                            case 4:
                                specialistInstance.SpHP = setPerfectionPacket.Value;
                                break;

                            case 5:
                                specialistInstance.SpFire = setPerfectionPacket.Value;
                                break;

                            case 6:
                                specialistInstance.SpWater = setPerfectionPacket.Value;
                                break;

                            case 7:
                                specialistInstance.SpLight = setPerfectionPacket.Value;
                                break;

                            case 8:
                                specialistInstance.SpDark = setPerfectionPacket.Value;
                                break;

                            default:
                                Session.SendPacket(Session.Character.GenerateSay(UpgradeCommandPacket.ReturnHelp(),
                                    10));
                                break;
                        }
                    }
                    else
                    {
                        Session.SendPacket(Session.Character.GenerateSay(UpgradeCommandPacket.ReturnHelp(), 10));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(UpgradeCommandPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Shout Command
        /// </summary>
        /// <param name="shoutPacket"></param>
        public void Shout(ShoutPacket shoutPacket)
        {
            if (shoutPacket != null)
            {

                CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
                {
                    DestinationCharacterId = null,
                    SourceCharacterId = Session.Character.CharacterId,
                    SourceWorldId = ServerManager.Instance.WorldId,
                    Message = $"{Session.Character.Name}: {shoutPacket.Message}",
                    Type = MessageType.Shout
                });
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ShoutPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $ShoutHere Command
        /// </summary>
        /// <param name="shoutHerePacket"></param>
        public void ShoutHere(ShoutHerePacket shoutHerePacket)
        {
            if (shoutHerePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[ShoutHere]Message: {shoutHerePacket.Message}");

                ServerManager.Shout(shoutHerePacket.Message);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ShoutHerePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Shutdown Command
        /// </summary>
        /// <param name="shutdownPacket"></param>
        public void Shutdown(ShutdownPacket shutdownPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Shutdown]");

            if (ServerManager.Instance.TaskShutdown != null)
            {
                ServerManager.Instance.ShutdownStop = true;
                ServerManager.Instance.TaskShutdown = null;
            }
            else
            {
                ServerManager.Instance.TaskShutdown = ServerManager.Instance.ShutdownTaskAsync();
                ServerManager.Instance.TaskShutdown.Start();
            }
        }

        /// <summary>
        /// $ShutdownAll Command
        /// </summary>
        /// <param name="shutdownAllPacket"></param>
        public void ShutdownAll(ShutdownAllPacket shutdownAllPacket)
        {
            if (shutdownAllPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[ShutdownAll]");

                if (!string.IsNullOrEmpty(shutdownAllPacket.WorldGroup))
                {
                    CommunicationServiceClient.Instance.Shutdown(shutdownAllPacket.WorldGroup, 5);
                }
                else
                {
                    CommunicationServiceClient.Instance.Shutdown(ServerManager.Instance.ServerGroup, 5);
                }

                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(ShutdownAllPacket.ReturnHelp(), 10));
            }
        }


        public void Gift(GiftPacket giftPacket)
        {
            if (giftPacket != null)
            {
                short Amount = giftPacket.Amount;

                if (Amount <= 0)
                {
                    Amount = 1;
                }


                if (giftPacket.CharacterName == "*")
                {
                    if (Session.HasCurrentMapInstance)
                    {
                        Parallel.ForEach(Session.CurrentMapInstance.Sessions,
                           session => Session.Character.SendGift(session.Character.CharacterId, giftPacket.VNum,
                                (short)Amount, giftPacket.Rare, giftPacket.Upgrade, giftPacket.Design, false));
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                    }
                }
                else if (giftPacket.CharacterName == "ALL")
                {
                    int levelMin = giftPacket.ReceiverLevelMin;
                    int levelMax = giftPacket.ReceiverLevelMax == 0 ? 99 : giftPacket.ReceiverLevelMax;

                    DAOFactory.CharacterDAO.LoadAll().ToList().ForEach(chara =>
                    {
                        if (chara.Level >= levelMin && chara.Level <= levelMax)
                        {
                            Session.Character.SendGift(chara.CharacterId, giftPacket.VNum, Amount,
                                giftPacket.Rare, giftPacket.Upgrade, giftPacket.Design, false);
                        }
                    });
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                }
                else
                {
                    CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(giftPacket.CharacterName);
                    if (chara != null)
                    {
                        Session.Character.SendGift(chara.CharacterId, giftPacket.VNum, (short)Amount,
                            giftPacket.Rare, giftPacket.Upgrade, giftPacket.Design, false);
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("GIFT_SENT"), 10));
                    }
                    else
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"),
                                0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(GiftPacket.ReturnHelp(), 10));
            }
        }

        ///// <summary>
        /// <summary>
        /// $Promote Command
        /// </summary>
        /// <param name="promotePacket"></param>
        public void Promote(PromotePacket promotePacket)
        {
            if (promotePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Promote]CharacterName: {promotePacket.CharacterName}");

                string name = promotePacket.CharacterName;
                try
                {
                    AccountDTO account = ServerManager.Instance.Sessions.FirstOrDefault(s => s.HasSelectedCharacter && s.Character.Name == name)?.Account;

                    if (account == null)
                    {
                        account = DAOFactory.AccountDAO.LoadById(DAOFactory.CharacterDAO.LoadByName(name).AccountId);
                    }

                    if (account != null && account.Authority >= AuthorityType.User)
                    {
                        if (account.Authority < Session.Account.Authority)
                        {
                            AuthorityType newAuthority;
                            switch (account.Authority)
                            {
                                case AuthorityType.User:
                                    newAuthority = AuthorityType.TGS;
                                    break;
                                case AuthorityType.TGS:
                                    newAuthority = AuthorityType.GS;
                                    break;
                                case AuthorityType.GS:
                                    newAuthority = AuthorityType.GM;
                                    break;
                                case AuthorityType.GM:
                                    newAuthority = AuthorityType.SGM;
                                    break;
                                case AuthorityType.SGM:
                                    newAuthority = AuthorityType.Administrator;
                                    break;
                                case AuthorityType.Administrator:
                                    newAuthority = AuthorityType.Founder;
                                    break;
                                case AuthorityType.Founder:
                                    newAuthority = AuthorityType.God;
                                    break;
                                default:
                                    newAuthority = account.Authority;
                                    break;
                            }
                            account.Authority = newAuthority;
                            DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                            ClientSession session =
                                ServerManager.Instance.Sessions.FirstOrDefault(s => s.Character?.Name == name);

                            if (session != null)
                            {

                                session.Account.Authority = newAuthority;

                                ServerManager.Instance.ChangeMap(session.Character.CharacterId);
                                DAOFactory.AccountDAO.WriteGeneralLog(session.Account.AccountId, session.IpAddress,
                                    session.Character.CharacterId, GeneralLogType.Promotion, $"by: {Session.Character.Name}");
                            }
                            else
                            {
                                DAOFactory.AccountDAO.WriteGeneralLog(account.AccountId, "127.0.0.1", null,
                                    GeneralLogType.Promotion, $"by: {Session.Character.Name}");
                            }

                            Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
                        }
                        else
                        {
                            Session.SendPacket(
                                Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("CANT_DO_THAT"), 10));
                        }

                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                    }
                }
                catch
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(PromotePacket.ReturnHelp(), 10));
            }
        }
        /// <summary>
        /// $Sort Command
        /// </summary>
        /// <param name="sortPacket"></param>
        public void Sort(SortPacket sortPacket)
        {
            if (sortPacket?.InventoryType.HasValue == true)
            {
                Logger.Log.LogUserEvent("USERCOMMAND", Session.GenerateIdentity(),
                    $"[Sort]InventoryType: {sortPacket.InventoryType}");

                if (sortPacket.InventoryType == InventoryType.Equipment
                    || sortPacket.InventoryType == InventoryType.Etc || sortPacket.InventoryType == InventoryType.Main)
                {
                    Session.Character.Inventory.Reorder(Session, sortPacket.InventoryType.Value);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SortPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Speed Command
        /// </summary>
        /// <param name="speedPacket"></param>
        public void Speed(SpeedPacket speedPacket)
        {
            if (speedPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Speed]Value: {speedPacket.Value}");

                if (speedPacket.Value < 60)
                {
                    Session.Character.Speed = speedPacket.Value;
                    Session.Character.IsCustomSpeed = true;
                    Session.SendPacket(Session.Character.GenerateCond());
                }
                if (speedPacket.Value == 0)
                {
                    Session.Character.IsCustomSpeed = false;
                    Session.Character.LoadSpeed();
                    Session.SendPacket(Session.Character.GenerateCond());
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SpeedPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $SPRefill Command
        /// </summary>
        /// <param name="spRefillPacket"></param>
        public void SpRefill(SPRefillPacket spRefillPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[SPRefill]");

            Session.Character.SpPoint = 10000;
            Session.Character.SpAdditionPoint = 1000000;
            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SP_REFILL"), 0));
            Session.SendPacket(Session.Character.GenerateSpPoint());
        }

        public void StartAct4(StartAct4Packet e)
        {
            Process.Start("OpenNos.World.exe", $"--nomsg --port 5100");
        }

        /// <summary>
        /// $Event Command
        /// </summary>
        /// <param name="eventPacket"></param>
        public void StartEvent(EventPacket eventPacket)
        {
            if (eventPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Event]EventType: {eventPacket.EventType.ToString()}");

                if (eventPacket.LvlBracket >= 0)
                {
                    EventHelper.GenerateEvent(eventPacket.EventType, eventPacket.LvlBracket);
                }
                else
                {
                    EventHelper.GenerateEvent(eventPacket.EventType);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(EventPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $GlobalEvent Command
        /// </summary>
        /// <param name="globalEventPacket"></param>
        public void StartGlobalEvent(GlobalEventPacket globalEventPacket)
        {
            if (globalEventPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[GlobalEvent]EventType: {globalEventPacket.EventType.ToString()}");

                CommunicationServiceClient.Instance.RunGlobalEvent(globalEventPacket.EventType);
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(EventPacket.ReturnHelp(), 10));
            }
        }


        /// <summary>
        /// $Stat Command
        /// </summary>
        /// <param name="statCommandPacket"></param>
        public void Stat(StatCommandPacket statCommandPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Stat]");

            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("XP_RATE_NOW")}: {ServerManager.Instance.Configuration.RateXP} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("DROP_RATE_NOW")}: {ServerManager.Instance.Configuration.RateDrop} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("GOLD_RATE_NOW")}: {ServerManager.Instance.Configuration.RateGold} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("GOLD_DROPRATE_NOW")}: {ServerManager.Instance.Configuration.RateGoldDrop} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("HERO_XPRATE_NOW")}: {ServerManager.Instance.Configuration.RateHeroicXP} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("FAIRYXP_RATE_NOW")}: {ServerManager.Instance.Configuration.RateFairyXP} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("REPUTATION_RATE_NOW")}: {ServerManager.Instance.Configuration.RateReputation} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ACT4XP_RATE_NOW")}: {ServerManager.Instance.Configuration.RateAct4Xp} ", 13));
            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("SERVER_WORKING_TIME")}: {(Process.GetCurrentProcess().StartTime - DateTime.Now).ToString(@"d\ hh\:mm\:ss")} ", 13));
            foreach (string message in CommunicationServiceClient.Instance.RetrieveServerStatistics())
            {
                Session.SendPacket(Session.Character.GenerateSay(message, 13));
            }
        }

        /// <summary>
        /// $Sudo Command
        /// </summary>
        /// <param name="sudoPacket"></param>
        public void SudoCommand(SudoPacket sudoPacket)
        {
            if (sudoPacket != null)
            {
                if (sudoPacket.CharacterName == "*")
                {
                    foreach (ClientSession sess in Session.CurrentMapInstance.Sessions.ToList().Where(s => s.Character?.Authority <= Session.Character.Authority))
                    {
                        sess.ReceivePacket(sudoPacket.CommandContents, true);
                    }
                }
                else
                {
                    ClientSession session = ServerManager.Instance.GetSessionByCharacterName(sudoPacket.CharacterName);

                    if (session != null && !string.IsNullOrWhiteSpace(sudoPacket.CommandContents))
                    {
                        if (session.Character?.Authority <= Session.Character.Authority)
                        {
                            session.ReceivePacket(sudoPacket.CommandContents, true);
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("CANT_DO_THAT"), 0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SudoPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Summon Command
        /// </summary>
        /// <param name="summonPacket"></param>
        public void Summon(SummonPacket summonPacket)
        {
            Random random = new();
            if (summonPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Summon]CharacterName: {summonPacket.CharacterName}");

                if (summonPacket.CharacterName == "*")
                {
                    Parallel.ForEach(
                        ServerManager.Instance.Sessions.Where(s =>
                            s.Character != null && s.Character.CharacterId != Session.Character.CharacterId), session =>
                        {
                            // clear any shop or trade on target character
                            Session.Character.DisposeShopAndExchange();
                            if (!session.Character.IsChangingMapInstance && Session.HasCurrentMapInstance)
                            {
                                List<MapCell> possibilities = new List<MapCell>();
                                for (short x = -6, y = -6; x < 6 && y < 6; x++, y++)
                                {
                                    possibilities.Add(new MapCell { X = x, Y = y });
                                }

                                short mapXPossibility = Session.Character.PositionX;
                                short mapYPossibility = Session.Character.PositionY;
                                foreach (MapCell possibility in possibilities.OrderBy(s => random.Next()))
                                {
                                    mapXPossibility = (short)(Session.Character.PositionX + possibility.X);
                                    mapYPossibility = (short)(Session.Character.PositionY + possibility.Y);
                                    if (!Session.CurrentMapInstance.Map.IsBlockedZone(mapXPossibility, mapYPossibility))
                                    {
                                        break;
                                    }
                                }

                                if (Session.Character.Miniland == Session.Character.MapInstance)
                                {
                                    ServerManager.Instance.JoinMiniland(session, Session);
                                }
                                else
                                {
                                    ServerManager.Instance.ChangeMapInstance(session.Character.CharacterId,
                                        Session.Character.MapInstanceId, mapXPossibility, mapYPossibility);
                                }
                            }
                        });
                }
                else
                {
                    ClientSession targetSession = ServerManager.Instance.GetSessionByCharacterName(summonPacket.CharacterName);
                    if (targetSession?.Character.IsChangingMapInstance == false)
                    {
                        Session.Character.DisposeShopAndExchange();
                        ServerManager.Instance.ChangeMapInstance(targetSession.Character.CharacterId,
                            Session.Character.MapInstanceId, (short)(Session.Character.PositionX + 1),
                            (short)(Session.Character.PositionY + 1));
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("USER_NOT_CONNECTED"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(SummonPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Teleport Command
        /// </summary>
        /// <param name="teleportPacket"></param>
        public void Teleport(TeleportPacket teleportPacket)
        {
            if (teleportPacket != null)
            {
                if (Session.Character.HasShopOpened || Session.Character.InExchangeOrTrade)
                {
                    Session.Character.DisposeShopAndExchange();
                }

                if (Session.Character.IsChangingMapInstance)
                {
                    return;
                }

                ClientSession session = ServerManager.Instance.GetSessionByCharacterName(teleportPacket.Data);

                if (session != null)
                {

                    short mapX = session.Character.PositionX;
                    short mapY = session.Character.PositionY;
                    if (session.Character.Miniland == session.Character.MapInstance)
                    {
                        ServerManager.Instance.JoinMiniland(Session, session);
                    }
                    else
                    {
                        ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                            session.Character.MapInstanceId, mapX, mapY);
                    }
                }
                else if (short.TryParse(teleportPacket.Data, out short mapId))
                {
                    Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Teleport]MapId: {teleportPacket.Data} MapX: {teleportPacket.X} MapY: {teleportPacket.Y}");

                    if (ServerManager.GetBaseMapInstanceIdByMapId(mapId) != default)
                    {
                        if (teleportPacket.X == 0 && teleportPacket.Y == 0)
                        {
                            ServerManager.Instance.TeleportOnRandomPlaceInMap(Session, ServerManager.GetBaseMapInstanceIdByMapId(mapId));
                        }
                        else
                        {
                            ServerManager.Instance.ChangeMap(Session.Character.CharacterId, mapId, teleportPacket.X, teleportPacket.Y);
                        }
                    }
                    else
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAP_NOT_FOUND"), 0));
                    }
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(TeleportPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Unban Command
        /// </summary>
        /// <param name="unbanPacket"></param>
        public void Unban(UnbanPacket unbanPacket)
        {
            if (unbanPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Unban]CharacterName: {unbanPacket.CharacterName}");

                string name = unbanPacket.CharacterName;
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);
                if (chara != null)
                {
                    PenaltyLogDTO log = ServerManager.Instance.PenaltyLogs.Find(s => s.AccountId == chara.AccountId && s.Penalty == PenaltyType.Banned && s.DateEnd > DateTime.Now);
                    if (log != null)
                    {
                        var account = DAOFactory.AccountDAO.LoadById(log.AccountId);

                        if (account == null)
                        {
                            return;
                        }

                        account.Authority = AuthorityType.User;
                        DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        Character.InsertOrUpdatePenalty(log);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"),
                            10));
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_BANNED"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(UnbanPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $rbunban Command
        /// </summary>
        /// <param name="rbunbanPacket"></param>
        public void RBUnban(RBUnbanPacket rbunbanPacket)
        {
            if (rbunbanPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[RBUnban]CharacterName: {rbunbanPacket.CharacterName}");

                string name = rbunbanPacket.CharacterName;
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);
                if (chara != null)
                {
                    PenaltyLogDTO log = ServerManager.Instance.PenaltyLogs.Find(s => s.AccountId == chara.AccountId && s.Penalty == PenaltyType.RainbowBan && s.DateEnd > DateTime.Now);
                    if (log != null)
                    {
                        var account = DAOFactory.AccountDAO.LoadById(log.AccountId);

                        if (account == null)
                        {
                            return;
                        }

                        account.Authority = AuthorityType.User;
                        DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        Character.InsertOrUpdatePenalty(log);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"),
                            10));
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_BANNED"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(RBUnbanPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $aotunban Command
        /// </summary>
        /// <param name="aotunbanPacket"></param>
        public void AOTUnban(AOTUnbanPacket aotunbanPacket)
        {
            if (aotunbanPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[AOTUnban]CharacterName: {aotunbanPacket.CharacterName}");

                string name = aotunbanPacket.CharacterName;
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);
                if (chara != null)
                {
                    PenaltyLogDTO log = ServerManager.Instance.PenaltyLogs.Find(s => s.AccountId == chara.AccountId && s.Penalty == PenaltyType.ArenaBan && s.DateEnd > DateTime.Now);
                    if (log != null)
                    {
                        var account = DAOFactory.AccountDAO.LoadById(log.AccountId);

                        if (account == null)
                        {
                            return;
                        }

                        account.Authority = AuthorityType.User;
                        DAOFactory.AccountDAO.InsertOrUpdate(ref account);
                        log.DateEnd = DateTime.Now.AddSeconds(-1);
                        Character.InsertOrUpdatePenalty(log);
                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"),
                            10));
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_BANNED"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(AOTUnbanPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Undercover Command
        /// </summary>
        /// <param name="undercoverPacket"></param>
        public void Undercover(UndercoverPacket undercoverPacket)
        {
            Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Undercover]");

            Session.Character.Undercover = !Session.Character.Undercover;
            ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId, Session.CurrentMapInstance.MapInstanceId, Session.Character.PositionX, Session.Character.PositionY);
        }

        /// <summary>
        /// $Unmute Command
        /// </summary>
        /// <param name="unmutePacket"></param>
        public void Unmute(UnmutePacket unmutePacket)
        {
            if (unmutePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Unmute]CharacterName: {unmutePacket.CharacterName}");

                string name = unmutePacket.CharacterName;
                CharacterDTO chara = DAOFactory.CharacterDAO.LoadByName(name);
                if (chara != null)
                {
                    if (ServerManager.Instance.PenaltyLogs.Any(s =>
                        s.AccountId == chara.AccountId && s.Penalty == (byte)PenaltyType.Muted
                        && s.DateEnd > DateTime.Now))
                    {
                        PenaltyLogDTO log = ServerManager.Instance.PenaltyLogs.Find(s =>
                            s.AccountId == chara.AccountId && s.Penalty == (byte)PenaltyType.Muted
                            && s.DateEnd > DateTime.Now);
                        if (log != null)
                        {
                            log.DateEnd = DateTime.Now.AddSeconds(-1);
                            Character.InsertOrUpdatePenalty(log);
                        }

                        Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"),
                            10));
                    }
                    else
                    {
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_MUTED"), 10));
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(UnmutePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Unstuck Command
        /// </summary>
        /// <param name="unstuckPacket"></param>
        public void Unstuck(UnstuckPacket unstuckPacket)
        {
            if (Session?.Character != null)
            {
                if (Session.CurrentMapInstance.MapInstanceType.Equals(MapInstanceType.RainbowBattleInstance))
                {
                    Session.SendPacket(StaticPacketHelper.Cancel(2));
                }

                if (Session.Character.Miniland == Session.Character.MapInstance)
                {
                    ServerManager.Instance.JoinMiniland(Session, Session);
                }
                else if (!Session.Character.IsSeal
                      && !Session.CurrentMapInstance.MapInstanceType.Equals(MapInstanceType.TalentArenaMapInstance)
                      && !Session.CurrentMapInstance.MapInstanceType.Equals(MapInstanceType.RainbowBattleInstance))
                {
                    ServerManager.Instance.ChangeMapInstance(Session.Character.CharacterId,
                        Session.Character.MapInstanceId, Session.Character.PositionX, Session.Character.PositionY,
                        true);
                    Session.SendPacket(StaticPacketHelper.Cancel(2));
                }
            }
        }

        /// <summary>
        /// $Upgrade Command
        /// </summary>
        /// <param name="upgradePacket"></param>
        public void Upgrade(UpgradeCommandPacket upgradePacket)
        {
            if (upgradePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Upgrade]Slot: {upgradePacket.Slot} Mode: {upgradePacket.Mode} Protection: {upgradePacket.Protection}");

                if (upgradePacket.Slot >= 0)
                {
                    ItemInstance wearableInstance =
                        Session.Character.Inventory.LoadBySlotAndType(upgradePacket.Slot, 0);
                    wearableInstance?.UpgradeItem(Session, upgradePacket.Mode, upgradePacket.Protection, true);
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(UpgradeCommandPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Warn Command
        /// </summary>
        /// <param name="warningPacket"></param>
        public void Warn(WarningPacket warningPacket)
        {
            if (warningPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[Warn]CharacterName: {warningPacket.CharacterName} Reason: {warningPacket.Reason}");

                string characterName = warningPacket.CharacterName;
                CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(characterName);
                if (character != null)
                {
                    ClientSession session = ServerManager.Instance.GetSessionByCharacterName(characterName);
                    session?.SendPacket(UserInterfaceHelper.GenerateInfo(
                        string.Format(Language.Instance.GetMessageFromKey("WARNING"), warningPacket.Reason)));
                    Character.InsertOrUpdatePenalty(new PenaltyLogDTO
                    {
                        AccountId = character.AccountId,
                        Reason = warningPacket.Reason,
                        Penalty = PenaltyType.Warning,
                        DateStart = DateTime.Now,
                        DateEnd = DateTime.Now,
                        AdminName = Session.Character.Name
                    });
                    switch (DAOFactory.PenaltyLogDAO.LoadByAccount(character.AccountId)
                        .Count(p => p.Penalty == PenaltyType.Warning))
                    {
                        case 1:
                            break;

                        case 2:
                            MuteMethod(characterName, "Auto-Warning mute: 2 strikes", 30);
                            break;

                        case 3:
                            MuteMethod(characterName, "Auto-Warning mute: 3 strikes", 60);
                            break;

                        case 4:
                            MuteMethod(characterName, "Auto-Warning mute: 4 strikes", 720);
                            break;

                        case 5:
                            MuteMethod(characterName, "Auto-Warning mute: 5 strikes", 1440);
                            break;

                        case 69:
                            BanMethod(characterName, 7, "LOL SIXTY NINE AMIRITE?");
                            break;

                        default:
                            MuteMethod(characterName, "You've been THUNDERSTRUCK",
                                6969); // imagined number as for I = √(-1), complex z = a + bi
                            break;
                    }
                }
                else
                {
                    Session.SendPacket(
                        Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"), 10));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(WarningPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $WigColor Command
        /// </summary>
        /// <param name="wigColorPacket"></param>
        public void WigColor(WigColorPacket wigColorPacket)
        {
            if (wigColorPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(),
                    $"[WigColor]Color: {wigColorPacket.Color}");

                ItemInstance wig =
                    Session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Hat, InventoryType.Wear);
                if (wig != null)
                {
                    wig.Design = wigColorPacket.Color;
                    Session.SendPacket(Session.Character.GenerateEquipment());
                    Session.SendPacket(Session.Character.GenerateEq());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateIn());
                    Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateGidx());
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_WIG"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(WigColorPacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $XpRate Command
        /// </summary>
        /// <param name="xpRatePacket"></param>
        public void XpRate(XpRatePacket xpRatePacket)
        {
            if (xpRatePacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[XpRate]Value: {xpRatePacket.Value}");

                if (xpRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.Configuration.RateXP = xpRatePacket.Value;

                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("XP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(XpRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Act4XpRate Command
        /// </summary>
        /// <param name="Act4xpRatePacket"></param>
        public void Act4XpRate(Act4XpRatePacket act4xpRatePacket)
        {
            if (act4xpRatePacket != null)
            {
                if (ServerManager.Instance.ChannelId != 51)
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_CHANNEL"), 0));
                    return;
                }
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[act4XpRatePacket]Value: {act4xpRatePacket.Value}");

                if (act4xpRatePacket.Value <= 1000)
                {
                    ServerManager.Instance.Configuration.RateAct4Xp = act4xpRatePacket.Value;

                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ACT4XP_RATE_CHANGED"), 0));
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_VALUE"), 0));
                }
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(XpRatePacket.ReturnHelp(), 10));
            }
        }

        /// <summary>
        /// $Zoom Command
        /// </summary>
        /// <param name="zoomPacket"></param>
        public void Zoom(ZoomPacket zoomPacket)
        {
            if (zoomPacket != null)
            {
                Logger.Log.LogUserEvent("GMCOMMAND", Session.GenerateIdentity(), $"[Zoom]Value: {zoomPacket.Value}");

                Session.SendPacket(
                    UserInterfaceHelper.GenerateGuri(15, zoomPacket.Value, Session.Character.CharacterId));
            }

            Session.Character.GenerateSay(ZoomPacket.ReturnHelp(), 10);
        }


        /// <summary>
        /// private addMate method
        /// </summary>
        /// <param name="vnum"></param>
        /// <param name="level"></param>
        /// <param name="mateType"></param>
        private void AddMate(short vnum, byte level, MateType mateType)
        {
            NpcMonster mateNpc = ServerManager.GetNpcMonster(vnum);
            if (Session.CurrentMapInstance == Session.Character.Miniland && mateNpc != null)
            {
                level = level == 0 ? (byte)1 : level;
                Mate mate = new Mate(Session.Character, mateNpc, level, mateType);
                Session.Character.AddPet(mate);
            }
            else
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_IN_MINILAND"), 0));
            }
        }

        /// <summary>
        /// private add portal command
        /// </summary>
        /// <param name="destinationMapId"></param>
        /// <param name="destinationX"></param>
        /// <param name="destinationY"></param>
        /// <param name="type"></param>
        /// <param name="insertToDatabase"></param>
        private void AddPortal(short destinationMapId, short destinationX, short destinationY, short type,
            bool insertToDatabase)
        {
            if (Session.HasCurrentMapInstance)
            {
                Portal portal = new Portal
                {
                    SourceMapId = Session.Character.MapId,
                    SourceX = Session.Character.PositionX,
                    SourceY = Session.Character.PositionY,
                    DestinationMapId = destinationMapId,
                    DestinationX = destinationX,
                    DestinationY = destinationY,
                    DestinationMapInstanceId = insertToDatabase ? Guid.Empty :
                        destinationMapId == 20000 ? Session.Character.Miniland.MapInstanceId : Guid.Empty,
                    Type = type
                };
                if (insertToDatabase)
                {
                    DAOFactory.PortalDAO.Insert(portal);
                }

                Session.CurrentMapInstance.Portals.Add(portal);
                Session.CurrentMapInstance?.Broadcast(portal.GenerateGp());
            }
        }

        /// <summary>
        /// private ban method
        /// </summary>
        /// <param name="characterName"></param>
        /// <param name="duration"></param>
        /// <param name="reason"></param>
        private void BanMethod(string characterName, int duration, string reason)
        {
            CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(characterName);
            if (character != null)
            {
                var account = DAOFactory.AccountDAO.LoadById(character.AccountId);

                if (account != null)
                {
                    CommunicationServiceClient.Instance.KickSession(account.AccountId, null);
                }

                ServerManager.Instance.Kick(characterName);
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = character.AccountId,
                    Reason = reason?.Trim(),
                    Penalty = PenaltyType.Banned,
                    DateStart = DateTime.Now,
                    DateEnd = duration == 0 ? DateTime.Now.AddYears(15) : DateTime.Now.AddDays(duration),
                    AdminName = Session.Character.Name
                };
                Character.InsertOrUpdatePenalty(log);
                ServerManager.Instance.BannedAccounts.Add(character.AccountId);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"),
                    10));
            }
        }

        /// <summary>
        /// private ban method
        /// </summary>
        /// <param name="rbbban"></param>
        public void RBBBan(RBBBanPacket rbbban)
        {
            CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(rbbban?.CharacterName);
            ClientSession session = ServerManager.Instance.GetSessionByCharacterName(rbbban?.CharacterName);

            if (character != null)
            {
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = character.AccountId,
                    Reason = rbbban.Reason == null ? "Griefing" : rbbban.Reason?.Trim(),
                    Penalty = PenaltyType.RainbowBan,
                    DateStart = DateTime.Now,
                    DateEnd = rbbban.Duration == 0 ? DateTime.Now.AddDays(5475) : DateTime.Now.AddDays(rbbban.Duration),
                    AdminName = Session?.Character.Name,
                };

                Session?.ReceivePacket($"pst 1 2 0 0 0 {rbbban?.CharacterName} RainbowBattleBan YougotbannedfromtheRainbowBattle!Reason:{(rbbban?.Reason != null ? rbbban.Reason.Trim().Replace(' ', '') : "Griefing")}EndofBan:{(rbbban.Duration == 0 ? DateTime.Now.AddDays(5475) : DateTime.Now.AddDays(rbbban.Duration))}!");
                session?.SendPacket($"info {Session.Character.Name} has banned you from Rainbow Battle till {(rbbban.Duration == 0 ? DateTime.Now.AddDays(5475) : DateTime.Now.AddDays(rbbban.Duration))} for {(rbbban?.Reason != null ? rbbban.Reason.Trim() : "Griefing")}");
                Character.InsertOrUpdatePenalty(log);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"),
                    10));
            }
        }

        /// <summary>
        /// private ban method
        /// </summary>
        /// <param name="aotban"></param>
        public void AOTBan(AOTBanPacket aoTBanPacket)
        {
            CharacterDTO character = DAOFactory.CharacterDAO.LoadByName(aoTBanPacket?.CharacterName);
            ClientSession session = ServerManager.Instance.GetSessionByCharacterName(aoTBanPacket?.CharacterName);

            if (character != null)
            {
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = character.AccountId,
                    Reason = aoTBanPacket.Reason == null ? "Griefing" : aoTBanPacket.Reason?.Trim(),
                    Penalty = PenaltyType.ArenaBan,
                    DateStart = DateTime.Now,
                    DateEnd = aoTBanPacket.Duration == 0 ? DateTime.Now.AddDays(5475) : DateTime.Now.AddDays(aoTBanPacket.Duration),
                    AdminName = Session?.Character.Name,
                };

                Session?.ReceivePacket($"pst 1 2 0 0 0 {aoTBanPacket?.CharacterName} ArenaOfTalentsBan YougotbannedfromAoT!Reason:{(aoTBanPacket?.Reason != null ? aoTBanPacket.Reason.Trim().Replace(' ', '') : "Griefing")}EndofBan:{(aoTBanPacket.Duration == 0 ? DateTime.Now.AddDays(5475) : DateTime.Now.AddDays(aoTBanPacket.Duration))}!");
                session?.SendPacket($"info {Session.Character.Name} has banned you from the Arena of Talents till {(aoTBanPacket.Duration == 0 ? DateTime.Now.AddDays(5475) : DateTime.Now.AddDays(aoTBanPacket.Duration))} for {(aoTBanPacket?.Reason != null ? aoTBanPacket.Reason.Trim() : "Griefing")}");
                Character.InsertOrUpdatePenalty(log);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"),
                    10));
            }
        }

        /// <summary>
        /// private mute method
        /// </summary>
        /// <param name="characterName"></param>
        /// <param name="reason"></param>
        /// <param name="duration"></param>
        private void MuteMethod(string characterName, string reason, int duration)
        {
            CharacterDTO characterToMute = DAOFactory.CharacterDAO.LoadByName(characterName);
            if (characterToMute != null)
            {
                ClientSession session = ServerManager.Instance.GetSessionByCharacterName(characterName);
                if (session?.Character.IsMuted() == false)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateInfo(
                        string.Format(Language.Instance.GetMessageFromKey("MUTED_PLURAL"), reason, duration)));
                }

                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = characterToMute.AccountId,
                    Reason = reason,
                    Penalty = PenaltyType.Muted,
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddMinutes(duration),
                    AdminName = Session.Character.Name
                };
                Character.InsertOrUpdatePenalty(log);
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("DONE"), 10));
            }
            else
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("USER_NOT_FOUND"),
                    10));
            }
        }

        /// <summary>
        /// Helper method used for sending stats of desired character
        /// </summary>
        /// <param name="characterDto"></param>
        private void SendStats(CharacterDTO characterDto)
        {
            Session.SendPacket(Session.Character.GenerateSay("----- CHARACTER -----", 13));

            Session.SendPacket(Session.Character.GenerateSay($"Last Save (disconnection): {characterDto.LastSave.ToString("dddd, dd MMMM yyyy HH:mm:ss")}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Name: {characterDto.Name}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Id: {characterDto.CharacterId}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"State: {characterDto.State}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Gender: {characterDto.Gender}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Class: {characterDto.Class}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Level: {characterDto.Level}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"JobLevel: {characterDto.JobLevel}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"HeroLevel: {characterDto.HeroLevel}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Gold: {characterDto.Gold}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Bio: {characterDto.Biography}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"MapId: {characterDto.MapId}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"MapX: {characterDto.MapX}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"MapY: {characterDto.MapY}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Reputation: {characterDto.Reputation}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Dignity: {characterDto.Dignity}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Rage: {characterDto.RagePoint}", 13));
            Session.SendPacket(Session.Character.GenerateSay($"Compliment: {characterDto.Compliment}", 13));
            Session.SendPacket(Session.Character.GenerateSay(
                $"Fraction: {(characterDto.Faction == FactionType.Demon ? Language.Instance.GetMessageFromKey("DEMON") : Language.Instance.GetMessageFromKey("ANGEL"))}",
                13));
            Session.SendPacket(Session.Character.GenerateSay("----- --------- -----", 13));
            AccountDTO account = DAOFactory.AccountDAO.LoadById(characterDto.AccountId);
            if (account != null)
            {
                Session.SendPacket(Session.Character.GenerateSay("----- ACCOUNT -----", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Id: {account.AccountId}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Name: {account.Name}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Authority: {account.Authority}", 13));
                Session.SendPacket(Session.Character.GenerateSay($"Bank gold: {account.GoldBank}", 13));

                if (Session.Account.Authority >= AuthorityType.GM)
                {
                    Session.SendPacket(Session.Character.GenerateSay($"RegistrationIP: {account.RegistrationIP}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"Email: {account.Email}", 13));
                }
                Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
                IEnumerable<PenaltyLogDTO> penaltyLogs = ServerManager.Instance.PenaltyLogs
                    .Where(s => s.AccountId == account.AccountId).ToList();
                PenaltyLogDTO penalty = penaltyLogs.LastOrDefault(s => s.DateEnd > DateTime.Now);
                Session.SendPacket(Session.Character.GenerateSay("----- PENALTY -----", 13));
                if (penalty != null)
                {
                    Session.SendPacket(Session.Character.GenerateSay($"Type: {penalty.Penalty}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"AdminName: {penalty.AdminName}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"Reason: {penalty.Reason}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"DateStart: {penalty.DateStart}", 13));
                    Session.SendPacket(Session.Character.GenerateSay($"DateEnd: {penalty.DateEnd}", 13));
                }

                Session.SendPacket(
                    Session.Character.GenerateSay($"Bans: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Banned)}",
                        13));
                Session.SendPacket(
                    Session.Character.GenerateSay($"Mutes: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Muted)}",
                        13));
                Session.SendPacket(
                    Session.Character.GenerateSay(
                        $"Warnings: {penaltyLogs.Count(s => s.Penalty == PenaltyType.Warning)}", 13));
                Session.SendPacket(Session.Character.GenerateSay("----- ------- -----", 13));
            }

            Session.SendPacket(Session.Character.GenerateSay("----- SESSION -----", 13));
            foreach (long[] connection in CommunicationServiceClient.Instance.RetrieveOnlineCharacters(characterDto
                .CharacterId))
            {
                if (connection != null)
                {
                    CharacterDTO character = DAOFactory.CharacterDAO.LoadById(connection[0]);
                    if (character != null)
                    {
                        Session.SendPacket(Session.Character.GenerateSay($"Character Name: {character.Name}", 13));
                        Session.SendPacket(Session.Character.GenerateSay($"ChannelId: {connection[1]}", 13));
                        Session.SendPacket(Session.Character.GenerateSay("-----", 13));
                    }
                }
            }

            Session.SendPacket(Session.Character.GenerateSay("----- ------------ -----", 13));
        }

        #endregion
    }
}