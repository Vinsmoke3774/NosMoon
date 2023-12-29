using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using NosByte.Shared;
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

namespace OpenNos.Handler.World.Bazaar
{
    public class GetBazaarPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        private static readonly KeepAliveClient _keepAliveClient = KeepAliveClient.Instance;
        private static readonly BazaarHttpClient _bazaarClient = BazaarHttpClient.Instance;

        public GetBazaarPacketHandler(ClientSession session) => Session = session;

        private bool CanUseBazaar(long bazaarId)
        {
            if (DateTime.Now < Session.Character.BazaarActionTimer.LastGetAction.AddSeconds(2))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("You need to wait a few seconds before buying an item again."));
                Session.Character.BazaarActionTimer.LastGetAction = DateTime.Now;
                return false;
            }

            if (!_keepAliveClient.IsBazaarOnline())
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo($"Uh oh, it looks like the bazaar server is offline ! Please inform a staff member about it as soon as possible !"));
                return false;
            }

            if (!Session.Character.CanUseNosBazaar())
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("INFO_BAZAAR")));
                return false;
            }

            if (_bazaarClient.GetItemState(new GetStateQuery { Id = bazaarId }))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo($"An error occurred while trying to update this item."));
                return false;
            }

            return true;
        }

        /// <summary>
        /// c_scalc packet
        /// </summary>
        /// <param name="packet"></param>
        public void GetBazaar(CScalcPacket packet)
        {
            if (!CanUseBazaar(packet.BazaarId))
            {
                return;
            }

            var bazaarItem = _bazaarClient.GetBazaarItem(new GetBazaarItemQuery() { Id = packet.BazaarId });

            if (!_bazaarClient.SetItemState(new SetStateCommand { Id = packet.BazaarId }))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo("An error occurred while trying to get this item from the bazaar."));
                return;
            }

            if (Session.Character == null || Session.Character.InExchangeOrTrade)
            {
                return;
            }
            
            if (ServerManager.Instance.InShutdown)
            {
                return;
            }

            if (bazaarItem == null)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateBazarRecollect(0, 0, 0, 0, 0, "None"));
                _bazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }
            var bazaarItemInstance = DAOFactory.ItemInstanceDAO.LoadById(bazaarItem.ItemInstanceId);

            if (bazaarItemInstance == null)
            {
                _bazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            var itemInstance = new ItemInstance(bazaarItemInstance);

            if (bazaarItem.SellerId != Session.Character.CharacterId)
            {
                _bazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            if ((bazaarItem.DateStart.AddHours(bazaarItem.Duration).AddDays(bazaarItem.MedalUsed ? 30 : 7) - DateTime.Now).TotalMinutes <= 0)
            {
                _bazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            var soldAmount = bazaarItem.Amount - itemInstance.Amount;
            var taxes = bazaarItem.MedalUsed ? 0 : (long)(bazaarItem.Price * 0.10 * soldAmount);
            var price = (bazaarItem.Price * soldAmount) - taxes;

            var name = itemInstance.Item?.Name ?? "None";

            if (itemInstance.Amount != 0 && !Session.Character.Inventory.CanAddItem(itemInstance.ItemVNum))
            {
                Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE")));
                Session.SendPacket(UserInterfaceHelper.GenerateBazarRecollect(bazaarItem.Price, 0, bazaarItem.Amount, 0, 0, name));
                _bazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            var addToBank = Session.Character.Gold + price > ServerManager.Instance.Configuration.MaxGold;
            var tooMuchGold = Session.Account.GoldBank + price > ServerManager.Instance.Configuration.MaxGoldBank;

            if (tooMuchGold)
            {
                Session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("MAX_GOLD"), 0));
                Session.SendPacket(UserInterfaceHelper.GenerateBazarRecollect(bazaarItem.Price, 0, bazaarItem.Amount, 0, 0, name));
                _bazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
                return;
            }

            if (addToBank)
            {
                if (price > 0)
                {
                    var modPrice = price % 1000;

                    if (modPrice > 0)
                    {
                        price -= modPrice;
                    }
                }

                Session.Account.GoldBank += price;
                Session.SendPacket(Session.Character.GenerateSay($"{price} gold were added to your bank account.", 11));
            }
            else
            {
                Session.Character.Gold += price;
            }

            Session.SendPacket(Session.Character.GenerateGold());
            Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("REMOVE_FROM_BAZAAR"), price), 10));

            // Edit this soo we dont generate new guid every single time we take
            // something out.
            if (itemInstance.Amount != 0)
            {
                var newItemInstance = itemInstance.DeepCopy();
                newItemInstance.Id = Guid.NewGuid();
                newItemInstance.Type = newItemInstance.Item.Type;

                newItemInstance.ShellEffects.AddRange(DAOFactory.ShellEffectDAO.LoadByEquipmentSerialId(itemInstance.Id));
                newItemInstance.ShellEffects.ForEach(s => s.EquipmentSerialId = newItemInstance.Id);
                newItemInstance.ShellEffects.ForEach(s => DAOFactory.ShellEffectDAO.InsertOrUpdate(s));

                Session.Character.Inventory.AddToInventory(newItemInstance);
            }

            Session.SendPacket(UserInterfaceHelper.GenerateBazarRecollect(bazaarItem.Price, soldAmount, bazaarItem.Amount, taxes, price, name));
            if (Session.Character.BazaarItems.ContainsKey(packet.BazaarId))
            {
                Session.Character.BazaarItems.TryRemove(packet.BazaarId, out _);
            }

            Logger.Log.LogUserEvent("BAZAAR_REMOVE", Session.GenerateIdentity(), $"BazaarId: {packet.BazaarId}, IId: {itemInstance.Id} VNum: {itemInstance.ItemVNum} Amount: {bazaarItem.Amount} RemainingAmount: {itemInstance.Amount} Price: {bazaarItem.Price}");

            var item = ServerManager.GetItem(bazaarItemInstance.ItemVNum);

            if (_bazaarClient.GetBazaarItem(new GetBazaarItemQuery() { Id = bazaarItem.BazaarItemId }) != null)
            {
                _bazaarClient.DeleteBazaarItem(new DeleteBazaarItemCommand() { Id = bazaarItem.BazaarItemId });
            }

            DAOFactory.ItemInstanceDAO.Delete(itemInstance.Id);
            Session.Character.Inventory.RemoveItemFromInventory(itemInstance.Id, itemInstance.Amount);
            new RefreshPersonalListPacketHandler(Session).RefreshPersonalBazarList(new CSListPacket());
            _bazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
            _bazaarClient.DeleteItemState(new DeleteStateCommand { Id = packet.BazaarId });
        }
    }
}
