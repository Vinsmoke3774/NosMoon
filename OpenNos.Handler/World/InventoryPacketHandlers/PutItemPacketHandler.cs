using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Helpers;
using System;
using System.Reactive.Linq;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class PutItemPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public PutItemPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// put packet
        /// </summary>
        /// <param name="putPacket"></param>
        public void PutItem(PutPacket putPacket)
        {
            Session.Character.LastDropRequests++;
            if (putPacket == null || Session.Character.HasShopOpened)
            {
                return;
            }

            if (putPacket.InventoryType == InventoryType.Wear || putPacket.InventoryType == InventoryType.Warehouse)
            {
                return;
            }

            if (Session.Character.LastDropRequests > 30)
            {
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = Session.Account.AccountId,
                    Reason = "Auto Ban PutRequest Infinite Abuse PL",
                    Penalty = PenaltyType.Banned,
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddYears(1),
                    AdminName = "NosMoon System"
                };
                Character.InsertOrUpdatePenalty(log);
                Session?.Disconnect();
                return;
            }
            Observable.Timer(TimeSpan.FromSeconds(5)).Subscribe(x =>
            {
                if (Session?.Character?.LastDropRequests > 0)
                {
                    Session.Character.LastDropRequests = 0;
                }
            });

            lock (Session.Character.Inventory)
            {
                ItemInstance invitem =
                    Session.Character.Inventory.LoadBySlotAndType(putPacket.Slot, putPacket.InventoryType);
                if (invitem?.Item.IsDroppable == true && invitem.Item.IsTradable
                    && !Session.Character.InExchangeOrTrade && putPacket.InventoryType != InventoryType.Bazaar)
                {
                    if (putPacket.Amount > 0 && putPacket.Amount < 10000)
                    {
                        if (Session.Character.MapInstance.DroppedList.Count < 200 && Session.HasCurrentMapInstance)
                        {
                            MapItem droppedItem = Session.CurrentMapInstance.PutItem(putPacket.InventoryType, putPacket.Slot, putPacket.Amount, ref invitem, Session);
                            if (droppedItem == null)
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE_HERE"), 0));
                                return;
                            }

                            if (putPacket.InventoryType == InventoryType.Wear) // tried glitch
                            {
                                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE"), 0));
                                return;
                            }

                            Session.SendPacket(invitem.GenerateInventoryAdd());

                            if (invitem.Amount == 0)
                            {
                                Session.Character.DeleteItem(invitem.Type, invitem.Slot);
                            }

                            Logger.Log.LogUserEvent("CHARACTER_ITEM_DROP", Session.GenerateIdentity(),
                                $"[PutItem]IIId: {invitem.Id} ItemVNum: {droppedItem.ItemVNum} Amount: {droppedItem.Amount} MapId: {Session.CurrentMapInstance.Map.MapId} MapX: {droppedItem.PositionX} MapY: {droppedItem.PositionY}");
                            Session.CurrentMapInstance?.Broadcast(
                                $"drop {droppedItem.ItemVNum} {droppedItem.TransportId} {droppedItem.PositionX} {droppedItem.PositionY} {droppedItem.Amount} 0 -1");
                        }
                        else
                        {
                            Session.SendPacket(
                                UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("DROP_MAP_FULL"),
                                    0));
                        }
                    }
                    else
                    {
                        Session.SendPacket(
                            UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_DROP_AMOUNT"), 0));
                    }
                }
                else
                {
                    Session.SendPacket(
                        UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ITEM_NOT_DROPPABLE"), 0));
                }
            }
        }
    }
}
