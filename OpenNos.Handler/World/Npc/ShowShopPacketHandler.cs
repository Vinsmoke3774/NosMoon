﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.RainbowBattle;
using OpenNos.Handler.SharedMethods;

namespace OpenNos.Handler.World.Npc
{
    public class ShowShopPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public ShowShopPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// npc_req packet
        /// </summary>
        /// <param name="requestNpcPacket"></param>
        public void ShowShop(RequestNpcPacket requestNpcPacket)
        {
            long owner = requestNpcPacket.Owner;
            if (!Session.HasCurrentMapInstance)
            {
                return;
            }

            if (requestNpcPacket.Type == 1)
            {
                // User Shop
                KeyValuePair<long, MapShop> shopList = Session.CurrentMapInstance.UserShops.FirstOrDefault(s => s.Value.OwnerId.Equals(owner));
                Session.LoadShopItem(owner, shopList);
            }
            else
            {
                // Npc Shop , ignore if has drop
                MapNpc npc = Session.CurrentMapInstance.Npcs.Find(n => n.MapNpcId.Equals((int)requestNpcPacket.Owner));
                if (npc == null)
                {
                    return;
                }

                if (Session.CurrentMapInstance.Map.MapId == 2010)
                {
                    var RainbowTeam = ServerManager.Instance.RainbowBattleMembers.Find(s => s.Session.Contains(Session));

                    if (RainbowTeam == null)
                    {
                        return;
                    }

                    if (RainbowBattleManager.AlreadyHaveFlag(RainbowTeam, (RainbowNpcType)npc.Score, npc.MapNpcId))
                    {
                        return;
                    }

                    Session.SendPacket(UserInterfaceHelper.GenerateDelay(3000, 1, $"#guri^720^{npc.Score}^{npc.MapNpcId}"));
                    return;
                }

                TeleporterDTO tp = npc.Teleporters?.FirstOrDefault(t => t?.Type == TeleporterType.TeleportOnMap);
                if (tp != null)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 1, $"#guri^710^{tp.MapX}^{tp.MapY}^{npc.MapNpcId}"));
                    return;
                }

                tp = npc.Teleporters?.FirstOrDefault(t => t?.Type == TeleporterType.TeleportOnOtherMap);
                if (tp != null)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 1, $"#guri^711^{tp.TeleporterId}"));
                    return;
                }

                if (npc.Owner != null && Session.Character.BattleEntity.IsSignpost(npc.NpcVNum))
                {
                    if (npc.Owner.Character?.Session != null && npc.Owner.Character?.Session == Session)
                    {
                        ServerManager.Instance.JoinMiniland(Session, npc.Owner.Character?.Session);
                    }
                    else
                    {
                        Session.SendPacket($"qna #mjoin^1^{npc.Owner.MapEntityId}^1  {npc.Owner.Character.Name} : {npc.Owner.Character.MinilandMessage} ");
                    }
                    return;
                }

                if (npc.Owner != null && Session.Character.BattleEntity.IsArenapost(npc.NpcVNum))
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateGuri(10, 3, 0));
                }

                if (npc.Owner != null && npc.BattleEntity.IsCampfire(npc.NpcVNum))
                {
                    short itemVNum = 0;
                    switch (npc.NpcVNum)
                    {
                        case 956:
                            itemVNum = 1375;
                            break;

                        case 957:
                            itemVNum = 1376;
                            break;

                        case 959:
                            itemVNum = 1437;
                            break;
                    }
                    if (itemVNum != 0)
                    {
                        Session.Character.LastItemVNum = itemVNum;
                        Session.SendPacket("wopen 27 0");
                        string recipelist = "m_list 2";
                        List<Recipe> recipeList = ServerManager.Instance.GetRecipesByItemVNum(itemVNum);
                        recipelist = recipeList.Where(s => s.Amount > 0).Aggregate(recipelist, (current, s) => current + $" {s.ItemVNum}");
                        recipelist += " -100";
                        Session.SendPacket(recipelist);
                    }
                }

                #region Quest

                if (Session.Character.Quests.FirstOrDefault(s => s.Quest.QuestRewards.Any(r => r.RewardType == 7 && r.Data == 5513)) is CharacterQuest sp6Quest
                 && sp6Quest.Quest.QuestObjectives.Any(s => s.SpecialData != null && s.Data == npc.NpcVNum && Session.Character.Inventory.CountItem(s.SpecialData.Value) >= s.Objective))
                {
                    short spVNum = 0;
                    switch (Session.Character.Class)
                    {
                        case ClassType.Swordsman:
                            spVNum = 4494;
                            break;

                        case ClassType.Archer:
                            spVNum = 4495;
                            break;

                        case ClassType.Magician:
                            spVNum = 4496;
                            break;
                    }
                    if (spVNum != 0)
                    {
                        ItemInstance newItem = new ItemInstance(spVNum, 1);
                        newItem.SpLevel = 80;
                        newItem.Upgrade = 10;
                        newItem.SlDamage = 152;
                        newItem.SlDefence = 0;
                        newItem.SlElement = 70;
                        newItem.SlHP = 38;

                        List<ItemInstance> newInv = Session.Character.Inventory.AddToInventory(newItem);
                        if (newInv.Count > 0)
                        {
                            Session.Character.Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {newItem.Item.Name}", 10));
                        }
                        else
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                            return;
                        }
                    }
                }

                Session.Character.IncrementQuests(QuestType.Dialog1, npc.NpcVNum);
                Session.Character.IncrementQuests(QuestType.Wear, npc.NpcVNum);
                Session.Character.IncrementQuests(QuestType.Brings, npc.NpcVNum);
                Session.Character.IncrementQuests(QuestType.Required, npc.NpcVNum);
                Session.Character.IncrementQuests(QuestType.TransmitGold, npc.NpcVNum);

                if (Session.Character.Quests.Any(s => s.QuestId == 6001 && npc.NpcVNum == 2051))
                {
                    Session.SendPacket($"npc_req 2 {npc.MapNpcId} 512");
                }

                if (Session.Character.LastQuest.AddSeconds(1) > DateTime.Now)
                {
                    return;
                }

                #endregion

                if (Session.Character.Quests.FirstOrDefault(s => s.Quest.DialogNpcVNum == npc.NpcVNum) is CharacterQuest npcDialogQuest && npcDialogQuest.Quest.DialogNpcId != null)
                {
                    Session.SendPacket($"npc_req 2 {npc.MapNpcId} {npcDialogQuest.Quest.DialogNpcId}");
                }
                else if (npc.NpcVNum == 856)
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 1, $"#guri^400^{npc.MapNpcId}"));
                }
                else if ((npc.Npc.Drops.Any(s => s.MonsterVNum != null) || npc.NpcVNum != 856) && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    if (npc.Npc.VNumRequired > 10 && Session.Character.Inventory.CountItem(npc.Npc.VNumRequired) < npc.Npc.AmountRequired)
                    {
                        if (ServerManager.GetItem(npc.Npc.VNumRequired) is Item requiredItem)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), npc.Npc.AmountRequired, requiredItem.Name), 0));
                            return;
                        }
                    }
                    Session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 4, $"#guri^400^{npc.MapNpcId}"));
                    Session.CurrentMapInstance?.Broadcast(Session, StaticPacketHelper.GenerateEff(UserType.Player, Session.Character.CharacterId, 5115), ReceiverType.AllNoEmoBlocked);
                }
                else if (npc.Npc.VNumRequired > 0 && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    if (npc.Npc.VNumRequired > 10 && Session.Character.Inventory.CountItem(npc.Npc.VNumRequired) < npc.Npc.AmountRequired)
                    {
                        if (ServerManager.GetItem(npc.Npc.VNumRequired) is Item requiredItem)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), npc.Npc.AmountRequired, requiredItem.Name), 0));
                            return;
                        }
                    }
                    Session.SendPacket(UserInterfaceHelper.GenerateDelay(6000, 4, $"#guri^400^{npc.MapNpcId}"));
                }
                else if (npc.Npc.MaxHP == 0 && npc.Npc.Drops.All(s => s.MonsterVNum == null) && npc.Npc.Race == 8 && (npc.Npc.RaceType == 7 || npc.Npc.RaceType == 5))
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateDelay(5000, 1, $"#guri^710^{npc.MapX}^{npc.MapY}^{npc.MapNpcId}")); // #guri^710^DestinationX^DestinationY^MapNpcId
                }
                else if (!string.IsNullOrEmpty(npc.GetNpcDialog()))
                {
                    Session.SendPacket(npc.GetNpcDialog());
                }
            }
        }
    }
}
