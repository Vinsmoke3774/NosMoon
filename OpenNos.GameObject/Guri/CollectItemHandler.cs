using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Actions;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.ActionHandles;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using OpenNos.GameObject.Npc;

namespace OpenNos.GameObject.Guri
{
    [GuriHandler(GuriType.CollectItem)]
    public class CollectItemHandler : SpecialHandlerBase, IGenericHandler<GuriPacket>
    {
        public CollectItemHandler(ClientSession session) : base(session)
        {
        }

        public void ValidateData(GuriPacket packet)
        {
            Execute(packet);
        }

        public void Execute(GuriPacket packet)
        {
            if (!Session.HasCurrentMapInstance)
            {
                return;
            }

            int mapNpcId = packet.Argument;

            MapNpc npc = Session.CurrentMapInstance.Npcs.Find(n => n.MapNpcId.Equals(mapNpcId));

            if (npc != null && !npc.IsOut)
            {
                NpcMonster mapobject = ServerManager.GetNpcMonster(npc.NpcVNum);

                int rateDrop = ServerManager.Instance.Configuration.RateDrop;
                int delay = (int)Math.Round(
                    (3 + (mapobject.RespawnTime / 1000d)) * Session.Character.TimesUsed);
                delay = delay > 11 ? 8 : delay;
                if (npc.NpcVNum == 1346 || npc.NpcVNum == 2350)
                {
                    delay = 0;
                }
                if (Session.Character.LastMapObject.AddSeconds(delay) < DateTime.Now)
                {
                    if (mapobject.Drops.Any(s => s.MonsterVNum != null) && mapobject.VNumRequired > 10
                        && Session.Character.Inventory.CountItem(mapobject.VNumRequired)
                        < mapobject.AmountRequired)
                    {
                        if (ServerManager.GetItem(mapobject.VNumRequired) is Item requiredItem)
                            Session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(
                                    string.Format(Language.Instance.GetMessageFromKey("NOT_ENOUGH_ITEMS"), mapobject.AmountRequired, requiredItem.Name), 0));
                        return;
                    }

                    Random random = new Random();
                    double randomAmount = ServerManager.RandomNumber() * random.NextDouble();
                    List<DropDTO> drops = mapobject.Drops.Where(s => s.MonsterVNum == npc.NpcVNum).ToList();
                    if (drops?.Count > 0)
                    {
                        int count = 0;
                        int probabilities = drops.Sum(s => s.DropChance);
                        int rnd = ServerManager.RandomNumber(0, probabilities);
                        int currentrnd = 0;
                        DropDTO firstDrop = mapobject.Drops.Find(s => s.MonsterVNum == npc.NpcVNum);

                        if (npc.NpcVNum == 2004 /* Ice Flower */ || npc.NpcVNum == 2020 && firstDrop != null)
                        {
                            ItemInstance newInv = Session.Character.Inventory.AddNewToInventory(firstDrop.ItemVNum, (short)firstDrop.Amount).FirstOrDefault();

                            if (newInv != null)
                            {
                                Session.Character.IncrementQuests(QuestType.Collect1, firstDrop.ItemVNum);
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), $"{newInv.Item.Name} x {firstDrop.Amount}"), 0));
                                Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"), $"{newInv.Item.Name} x {firstDrop.Amount}"), 11));
                            }
                            else
                            {
                                Session.Character.GiftAdd(firstDrop.ItemVNum, (short)firstDrop.Amount);
                            }

                            Session.CurrentMapInstance.Broadcast(npc.GenerateOut());

                            return;
                        }
                        else if (randomAmount * 1000 <= probabilities)
                        {
                            foreach (DropDTO drop in drops.OrderBy(s => ServerManager.RandomNumber()))
                            {
                                short vnum = drop.ItemVNum;
                                short amount = (short)drop.Amount;
                                int dropChance = drop.DropChance;
                                currentrnd += drop.DropChance;
                                if (currentrnd >= rnd)
                                {
                                    ItemInstance newInv = Session.Character.Inventory.AddNewToInventory(vnum, amount)
                                        .FirstOrDefault();
                                    if (newInv != null)
                                    {
                                        if (dropChance != 100000)
                                        {
                                            Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                                                string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"),
                                                    $"{newInv.Item.Name} x {amount}"), 0));
                                        }
                                        Session.SendPacket(Session.Character.GenerateSay(
                                            string.Format(Language.Instance.GetMessageFromKey("RECEIVED_ITEM"),
                                                $"{newInv.Item.Name} x {amount}"), 11));
                                        Session.Character.IncrementQuests(QuestType.Collect1, vnum);
                                    }
                                    else
                                    {
                                        Session.Character.GiftAdd(vnum, amount);
                                    }
                                    count++;
                                    if (dropChance != 100000)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        if (count > 0)
                        {
                            Session.Character.LastMapObject = DateTime.Now;
                            Session.Character.TimesUsed++;
                            if (Session.Character.TimesUsed >= 4)
                            {
                                Session.Character.TimesUsed = 0;
                            }

                            if (mapobject.VNumRequired > 10)
                            {
                                Session.Character.Inventory.RemoveItemAmount(npc.Npc.VNumRequired, npc.Npc.AmountRequired);
                            }

                            if (npc.NpcVNum == 1346 || npc.NpcVNum == 2350)
                            {
                                npc.SetDeathStatement();
                                npc.RunDeathEvent();
                                Session.Character.MapInstance.Broadcast(npc.GenerateOut());
                            }
                        }
                        else
                        {
                            Session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("TRY_FAILED"), 0));
                        }
                    }
                    else if (Session.CurrentMapInstance.Npcs.Where(s => s.Npc.Race == 8 && s.Npc.RaceType == 5 && s.MapNpcId != npc.MapNpcId) is IEnumerable<MapNpc> mapTeleportNpcs)
                    {
                        if (npc.Npc.VNumRequired > 0 && npc.Npc.AmountRequired > 0)
                        {
                            if (Session.Character.Inventory.CountItem(npc.Npc.VNumRequired) >= npc.Npc.AmountRequired)
                            {
                                if (npc.Npc.AmountRequired > 1)
                                {
                                    Session.Character.Inventory.RemoveItemAmount(npc.Npc.VNumRequired, npc.Npc.AmountRequired);
                                }
                            }
                            else
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NO_ITEM_REQUIRED"), 0));
                                return;
                            }
                        }
                        if (DAOFactory.TeleporterDAO.LoadFromNpc(npc.MapNpcId).FirstOrDefault() is TeleporterDTO teleport)
                        {
                            Session.Character.PositionX = teleport.MapX;
                            Session.Character.PositionY = teleport.MapY;
                            Session.CurrentMapInstance.Broadcast(Session.Character.GenerateTp());
                            foreach (Mate mate in Session.Character.Mates.Where(s => s.IsTeamMember && s.IsAlive))
                            {
                                mate.PositionX = teleport.MapX;
                                mate.PositionY = teleport.MapY;
                                Session.CurrentMapInstance.Broadcast(mate.GenerateTp());
                            }
                        }
                        else
                        {
                            MapNpc nearestTeleportNpc = null;
                            foreach (MapNpc teleportNpc in mapTeleportNpcs)
                            {
                                if (nearestTeleportNpc == null)
                                {
                                    nearestTeleportNpc = teleportNpc;
                                }
                                else if (
                                    Map.GetDistance(
                                    new MapCell { X = npc.MapX, Y = npc.MapY },
                                    new MapCell { X = teleportNpc.MapX, Y = teleportNpc.MapY })
                                    <
                                    Map.GetDistance(
                                    new MapCell { X = npc.MapX, Y = npc.MapY },
                                    new MapCell { X = nearestTeleportNpc.MapX, Y = nearestTeleportNpc.MapY }))
                                {
                                    nearestTeleportNpc = teleportNpc;
                                }
                            }
                            if (nearestTeleportNpc != null)
                            {
                                Session.Character.PositionX = nearestTeleportNpc.MapX;
                                Session.Character.PositionY = nearestTeleportNpc.MapY;
                                Session.CurrentMapInstance.Broadcast(Session.Character.GenerateTp());
                                foreach (Mate mate in Session.Character.Mates.Where(s => s.IsTeamMember && s.IsAlive))
                                {
                                    mate.PositionX = nearestTeleportNpc.MapX;
                                    mate.PositionY = nearestTeleportNpc.MapY;
                                    Session.CurrentMapInstance.Broadcast(mate.GenerateTp());
                                }
                            }
                        }
                    }
                }
                else
                {
                    Session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        string.Format(Language.Instance.GetMessageFromKey("TRY_FAILED_WAIT"),
                            (int)(Session.Character.LastMapObject.AddSeconds(delay) - DateTime.Now)
                            .TotalSeconds), 0));
                }
            }
        }
    }
}
