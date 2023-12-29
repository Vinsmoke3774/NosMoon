using JetBrains.Annotations;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.HttpClients;
using OpenNos.GameObject.Modules.Bazaar.Commands;
using OpenNos.GameObject.Modules.Bazaar.Queries;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace OpenNos.Handler.World.Bazaar
{
    [UsedImplicitly]
    public class BuyBazaarPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; }

        private static readonly KeepAliveClient KeepAliveClient = KeepAliveClient.Instance;
        private static readonly BazaarHttpClient BazaarClient = BazaarHttpClient.Instance;

        public BuyBazaarPacketHandler(ClientSession session) => Session = session;

        private bool CanUseBazaar(long bazaarId)
        {
            if (DateTime.Now < Session.Character.BazaarActionTimer.LastBuyAction.AddSeconds(2))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("You need to wait a few seconds before buying an item again."));
                return false;
            }

            Session.Character.BazaarActionTimer.LastBuyAction = DateTime.Now;

            if (!KeepAliveClient.IsBazaarOnline())
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("Uh oh, it looks like the bazaar server is offline ! Please inform a staff member about it as soon as possible !"));
                return false;
            }

            if (BazaarClient.GetItemState(new GetStateQuery { Id = bazaarId }))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("An error occurred while trying to update this item."));
                return false;
            }

            return true;
        }

        /// <summary>
        /// c_buy packet
        /// </summary>
        /// <param name="packet"></param>
        [UsedImplicitly]
        public void BuyBazaar(CBuyPacket packet)
        {
            Session.Character.BazarRequests++;
            if (!CanUseBazaar(packet.BazaarId))
            {
                return;
            }

            if (!BazaarClient.SetItemState(new SetStateCommand { Id = packet.BazaarId }))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("An error occurred while trying to buy this item."));
                return;
            }

            BazaarItemDTO bz = BazaarClient.GetBazaarItem(new GetBazaarItemQuery() { Id = packet.BazaarId });
            if (bz == null || packet.Amount <= 0)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("STATE_CHANGED"), 1));
                BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            if (Session.Character.InExchangeOrTrade || Session.Character.HasShopOpened)
            {
                return;
            }

            if (Session.Character.BazarRequests > 20)
            {
                PenaltyLogDTO log = new PenaltyLogDTO
                {
                    AccountId = Session.Account.AccountId,
                    Reason = "Auto Ban C_Buy Attempted PL",
                    Penalty = PenaltyType.Banned,
                    DateStart = DateTime.Now,
                    DateEnd = DateTime.Now.AddYears(1),
                    AdminName = "NosMoon System"
                };
                Character.InsertOrUpdatePenalty(log);
                Session?.Disconnect();
                return;
            }
            Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(x =>
            {
                if (Session?.Character?.BazarRequests > 0)
                    Session.Character.BazarRequests = 0;
            });

            long price = packet.Amount * bz.Price;

            var owner = DAOFactory.CharacterDAO.LoadById(bz.SellerId);
            var inventoryGold = Session.Character.Gold;
            var bankGold = Session.Account.GoldBank;
            bool hasEnoughGold = false;
            bool useBankGold = false;

            if (inventoryGold > price)
            {
                hasEnoughGold = true;
            }
            else if (bankGold > price)
            {
                hasEnoughGold = true;
                useBankGold = true;
            }

            if (!hasEnoughGold)
            {
                Session.SendPacket(Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 10));
                Session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("NOT_ENOUGH_MONEY"), 1));
                BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            BazaarItemLink createdBazaarItem = new() { BazaarItem = bz };
            if (DAOFactory.CharacterDAO.LoadById(bz.SellerId) != null)
            {
                createdBazaarItem.Owner = owner?.Name;
                createdBazaarItem.Item = new ItemInstance(DAOFactory.ItemInstanceDAO.LoadById(bz.ItemInstanceId));
            }
            else
            {
                BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            if (packet.Amount > createdBazaarItem.Item.Amount)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateModal(Language.Instance.GetMessageFromKey("STATE_CHANGED"), 1));
                BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            if (!Session.Character.Inventory.CanAddItem(createdBazaarItem.Item.ItemVNum))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"), 0));
                BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            if (bz.Price != packet.Price)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("WRONG_PRICE"), 0));
                BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            if (createdBazaarItem.Item == null)
            {
                return;
            }

            if (bz.IsPackage && packet.Amount != bz.Amount)
            {
                BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            ItemInstanceDTO bazaarItemInstance = DAOFactory.ItemInstanceDAO.LoadById(createdBazaarItem.BazaarItem.ItemInstanceId);
            if (bazaarItemInstance.Amount < packet.Amount)
            {
                BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            ItemInstance newBz = createdBazaarItem.Item.DeepCopy();
            newBz.Id = Guid.NewGuid();
            newBz.Amount = packet.Amount;
            newBz.Type = newBz.Item.Type;

            newBz.ShellEffects.AddRange(DAOFactory.ShellEffectDAO.LoadByEquipmentSerialId(createdBazaarItem.Item.Id));
            newBz.ShellEffects.ForEach(s => s.EquipmentSerialId = newBz.Id);
            newBz.ShellEffects.ForEach(s => DAOFactory.ShellEffectDAO.InsertOrUpdate(s));

            List<ItemInstance> newInv = Session.Character.Inventory.AddToInventory(newBz);

            if (newInv.Count == 0 || newInv.Count <= 0)
            {
                return;
            }

            bazaarItemInstance.Amount -= packet.Amount;

            if (!useBankGold)
            {
                Session.Character.Gold -= price;
            }
            else
            {
                if (price > 0)
                {
                    var modPrice = price % 1000;

                    if (modPrice > 0)
                    {
                        /*
                         * This is to avoid having weird values that are > 0 when % 1000 in db. It causes various bugs including exchange bug.
                         * Ex: price = 196 635 235
                         * new price = 166 636 235
                         * divided by 1000 -> 166 636
                         * multiplied by 1000 -> 166 636 000
                         */
                        price = ((price + 1000) / 1000) * 1000;
                    }
                }

                Session.Account.GoldBank -= price;
                Session.SendPacket(Session.Character.GenerateSay($"{price} gold were removed from your bank account.", 11));
            }

            Session.SendPacket(Session.Character.GenerateGold());
            DAOFactory.ItemInstanceDAO.InsertOrUpdate(bazaarItemInstance);
            Session.SendPacket($"rc_buy 1 {createdBazaarItem.Item.Item.VNum} {createdBazaarItem.Owner} {packet.Amount} {packet.Price} 0 0 0");
            BazaarClient.InsertOrUpdateBazaar(new InsertOrUpdateBazaarItemCommand() { BazaarItem = bz });

            Session.SendPacket(Session.Character.GenerateSay($"{Language.Instance.GetMessageFromKey("ITEM_ACQUIRED")}: {createdBazaarItem.Item.Item.Name} x {packet.Amount}", 10));

            CommunicationServiceClient.Instance.SendMessageToCharacter(new SCSCharacterMessage
            {
                DestinationCharacterId = bz.SellerId,
                SourceWorldId = ServerManager.Instance.WorldId,
                Message = StaticPacketHelper.Say(1, bz.SellerId, 12, string.Format(Language.Instance.GetMessageFromKey("BAZAAR_ITEM_SOLD"), packet.Amount, createdBazaarItem.Item.Item.Name)),
                Type = MessageType.Other
            });

            Logger.Log.LogUserEvent("BAZAAR_BUY", Session.GenerateIdentity(), $"BazaarId: {packet.BazaarId} VNum: {packet.VNum} Amount: {packet.Amount} Price: {packet.Price}");
            BazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
        }
    }
}
