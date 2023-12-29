using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.GameObject;

namespace OpenNos.Handler.World.Miniland
{
    public class MiniGamePlayPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public MiniGamePlayPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// mg packet
        /// </summary>
        /// <param name="packet"></param>
        public void MinigamePlay(MinigamePacket packet)
        {
            //Minigames desactivated
#if DEBUG
            if (packet != null)
            {
                Session?.SendPacket("info Miniland Games disabled.");
                return;
            }

            //ClientSession client =
            //    ServerManager.Instance.Sessions.FirstOrDefault(s =>
            //        s.Character?.Miniland == Session.Character.MapInstance);
            //MinilandObject mlobj =
            //    client?.Character.MinilandObjects.Find(s => s.ItemInstance.ItemVNum == packet.MinigameVNum);
            //if (mlobj != null)
            //{
            //    const bool full = false;
            //    byte game = (byte)(mlobj.ItemInstance.Item.EquipmentSlot);
            //    switch (packet.Type)
            //    {
            //        //play
            //        case 1:
            //            if (mlobj.ItemInstance.DurabilityPoint <= 0)
            //            {
            //                Session.SendPacket(UserInterfaceHelper.GenerateMsg(
            //                    Language.Instance.GetMessageFromKey("NOT_ENOUGH_DURABILITY_POINT"), 0));
            //                return;
            //            }

            //            if (Session.Character.MinilandPoint <= 0)
            //            {
            //                Session.SendPacket(
            //                    $"qna #mg^1^7^3125^1^1 {Language.Instance.GetMessageFromKey("NOT_ENOUGH_MINILAND_POINT")}");
            //            }

            //            Session.Character.MapInstance.Broadcast(
            //                UserInterfaceHelper.GenerateGuri(2, 1, Session.Character.CharacterId));
            //            Session.Character.CurrentMinigame = (short)(game == 0 ? 5102 :
            //                game == 1 ? 5103 :
            //                game == 2 ? 5105 :
            //                game == 3 ? 5104 :
            //                game == 4 ? 5113 : 5112);
            //            Session.Character.MinigameLog = new MinigameLogDTO
            //            {
            //                CharacterId = Session.Character.CharacterId,
            //                StartTime = DateTime.Now.Ticks,
            //                Minigame = game
            //            };
            //            Session.SendPacket($"mlo_st {game}");
            //            break;

            //        //stop
            //        case 2:
            //            Session.Character.CurrentMinigame = 0;
            //            Session.Character.MapInstance.Broadcast(
            //                UserInterfaceHelper.GenerateGuri(6, 1, Session.Character.CharacterId));
            //            break;

            //        case 3:
            //            Session.Character.CurrentMinigame = 0;
            //            Session.Character.MapInstance.Broadcast(
            //                UserInterfaceHelper.GenerateGuri(6, 1, Session.Character.CharacterId));
            //            if (packet.Point.HasValue && Session.Character.MinigameLog != null)
            //            {
            //                Session.Character.MinigameLog.EndTime = DateTime.Now.Ticks;
            //                Session.Character.MinigameLog.Score = packet.Point.Value;

            //                int level = -1;
            //                for (short i = 0; i < SharedMinilandMethods.GetMinilandMaxPoint(game).Length; i++)
            //                {
            //                    if (packet.Point > SharedMinilandMethods.GetMinilandMaxPoint(game)[i])
            //                    {
            //                        level = i;
            //                    }
            //                    else
            //                    {
            //                        break;
            //                    }
            //                }

            //                Session.SendPacket(level != -1
            //                    ? $"mlo_lv {level}"
            //                    : $"mg 3 {game} {packet.MinigameVNum} 0 0");
            //            }

            //            break;

            //        // select gift
            //        case 4:
            //            if (Session.Character.MinilandPoint >= 100 && Session.Character.MinigameLog != null
            //                && packet.Point.HasValue && packet.Point > 0)
            //            {
            //                if (SharedMinilandMethods.GetMinilandMaxPoint(game)[packet.Point.Value - 1] < Session.Character.MinigameLog.Score)
            //                {
            //                    MinigameLogDTO dto = Session.Character.MinigameLog;
            //                    DAOFactory.MinigameLogDAO.InsertOrUpdate(ref dto);
            //                    Session.Character.MinigameLog = null;
            //                    Gift obj = GetMinilandGift(packet.MinigameVNum, (int)packet.Point);
            //                    if (obj != null)
            //                    {
            //                        Session.SendPacket($"mlo_rw {obj.VNum} {obj.Amount}");
            //                        Session.SendPacket(Session.Character.GenerateMinilandPoint());
            //                        List<ItemInstance> inv =
            //                            Session.Character.Inventory.AddNewToInventory(obj.VNum, obj.Amount);
            //                        Session.Character.MinilandPoint -= 100;
            //                        if (inv.Count == 0)
            //                        {
            //                            Session.Character.SendGift(Session.Character.CharacterId, obj.VNum, obj.Amount, 0, 0, 0, false);
            //                        }

            //                        if (client != Session)
            //                        {
            //                            switch (packet.Point)
            //                            {
            //                                case 0:
            //                                    mlobj.Level1BoxAmount++;
            //                                    break;

            //                                case 1:
            //                                    mlobj.Level2BoxAmount++;
            //                                    break;

            //                                case 2:
            //                                    mlobj.Level3BoxAmount++;
            //                                    break;

            //                                case 3:
            //                                    mlobj.Level4BoxAmount++;
            //                                    break;

            //                                case 4:
            //                                    mlobj.Level5BoxAmount++;
            //                                    break;
            //                            }
            //                        }
            //                    }
            //                }
            //            }

            //            break;

            //        case 5:
            //            Session.SendPacket(Session.Character.GenerateMloMg(mlobj, packet));
            //            break;

            //        //refill
            //        case 6:
            //            if (packet.Point == null || packet.Point < 0)
            //            {
            //                return;
            //            }

            //            if (Session.Character.Gold > packet.Point)
            //            {
            //                Session.Character.Gold -= (int)packet.Point;
            //                Session.SendPacket(Session.Character.GenerateGold());
            //                mlobj.ItemInstance.DurabilityPoint += (int)(packet.Point / 100);
            //                Session.SendPacket(UserInterfaceHelper.GenerateInfo(
            //                    string.Format(Language.Instance.GetMessageFromKey("REFILL_MINIGAME"),
            //                        (int)packet.Point / 100)));
            //                Session.SendPacket(Session.Character.GenerateMloMg(mlobj, packet));
            //            }

            //            break;

            //        //gift
            //        case 7:
            //            Session.SendPacket(
            //                $"mlo_pmg {packet.MinigameVNum} {Session.Character.MinilandPoint} {(mlobj.ItemInstance.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} {(mlobj.Level1BoxAmount > 0 ? $"392 {mlobj.Level1BoxAmount}" : "0 0")} {(mlobj.Level2BoxAmount > 0 ? $"393 {mlobj.Level2BoxAmount}" : "0 0")} {(mlobj.Level3BoxAmount > 0 ? $"394 {mlobj.Level3BoxAmount}" : "0 0")} {(mlobj.Level4BoxAmount > 0 ? $"395 {mlobj.Level4BoxAmount}" : "0 0")} {(mlobj.Level5BoxAmount > 0 ? $"396 {mlobj.Level5BoxAmount}" : "0 0")} 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0");
            //            break;

            //        //get gift
            //        case 8:
            //            int amount = 0;
            //            switch (packet.Point)
            //            {
            //                case 0:
            //                    amount = mlobj.Level1BoxAmount;
            //                    break;

            //                case 1:
            //                    amount = mlobj.Level2BoxAmount;
            //                    break;

            //                case 2:
            //                    amount = mlobj.Level3BoxAmount;
            //                    break;

            //                case 3:
            //                    amount = mlobj.Level4BoxAmount;
            //                    break;

            //                case 4:
            //                    amount = mlobj.Level5BoxAmount;
            //                    break;
            //            }

            //            List<Gift> gifts = new List<Gift>();
            //            for (int i = 0; i < amount; i++)
            //            {
            //                if (packet.Point != null)
            //                {
            //                    Gift gift = GetMinilandGift(packet.MinigameVNum, (int)packet.Point);
            //                    if (gift != null)
            //                    {
            //                        if (gifts.Any(o => o.VNum == gift.VNum))
            //                        {
            //                            gifts.First(o => o.Amount == gift.Amount).Amount += gift.Amount;
            //                        }
            //                        else
            //                        {
            //                            gifts.Add(gift);
            //                        }
            //                    }
            //                }
            //            }

            //            string str = "";
            //            for (int i = 0; i < 9; i++)
            //            {
            //                if (gifts.Count > i)
            //                {
            //                    short itemVNum = gifts[i].VNum;
            //                    short itemAmount = gifts[i].Amount;
            //                    List<ItemInstance> inv =
            //                        Session.Character.Inventory.AddNewToInventory(itemVNum, itemAmount);
            //                    if (inv.Count > 0)
            //                    {
            //                        Session.SendPacket(Session.Character.GenerateSay(
            //                            $"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {ServerManager.GetItem(itemVNum).Name} x {itemAmount}",
            //                            12));
            //                    }
            //                    else
            //                    {
            //                        Session.Character.SendGift(Session.Character.CharacterId, itemVNum, itemAmount, 0, 0, 0, false);
            //                    }

            //                    str += $" {itemVNum} {itemAmount}";
            //                }
            //                else
            //                {
            //                    str += " 0 0";
            //                }
            //            }

            //            Session.SendPacket(
            //                $"mlo_pmg {packet.MinigameVNum} {Session.Character.MinilandPoint} {(mlobj.ItemInstance.DurabilityPoint < 1000 ? 1 : 0)} {(full ? 1 : 0)} {(mlobj.Level1BoxAmount > 0 ? $"392 {mlobj.Level1BoxAmount}" : "0 0")} {(mlobj.Level2BoxAmount > 0 ? $"393 {mlobj.Level2BoxAmount}" : "0 0")} {(mlobj.Level3BoxAmount > 0 ? $"394 {mlobj.Level3BoxAmount}" : "0 0")} {(mlobj.Level4BoxAmount > 0 ? $"395 {mlobj.Level4BoxAmount}" : "0 0")} {(mlobj.Level5BoxAmount > 0 ? $"396 {mlobj.Level5BoxAmount}" : "0 0")}{str}");
            //            break;

            //        //coupon
            //        case 9:
            //            List<ItemInstance> items = Session.Character.Inventory
            //                .Where(s => s.ItemVNum == 1269 || s.ItemVNum == 1271).OrderBy(s => s.Slot).ToList();
            //            if (items.Count > 0)
            //            {
            //                short itemVNum = items[0].ItemVNum;
            //                Session.Character.Inventory.RemoveItemAmount(itemVNum);
            //                int point = itemVNum == 1269 ? 300 : 500;
            //                mlobj.ItemInstance.DurabilityPoint += point;
            //                Session.SendPacket(UserInterfaceHelper.GenerateInfo(
            //                    string.Format(Language.Instance.GetMessageFromKey("REFILL_MINIGAME"), point)));
            //                Session.SendPacket(Session.Character.GenerateMloMg(mlobj, packet));
            //            }

            //            break;
            //    }
            //}

#endif
        }
    }
}
