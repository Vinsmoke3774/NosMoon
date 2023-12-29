using NosByte.Packets.ClientPackets.Inventory;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Networking;
using OpenNos.Handler.SharedMethods;
using System.Linq;

namespace OpenNos.Handler.World.InventoryPacketHandlers
{
    public class ExchangeListPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public ExchangeListPacketHandler(ClientSession session) => Session = session;

        public void ExchangeList(ExchangeListPacket packet)
        {
            Logger.Log.LogUserEvent("EXC_LIST", Session.GenerateIdentity(), $"Packet string: {packet.ToString()}");

            if (Session.Character.ExchangeInfo == null)
            {
                return;
            }

            if (Session.Character.ExchangeInfo.Gold != 0)
            {
                return;
            }

            if (Session.Character.ExchangeInfo.BankGold != 0)
            {
                return;
            }

            ClientSession targetSession =
                ServerManager.Instance.GetSessionByCharacterId(Session.Character.ExchangeInfo.TargetCharacterId);
            if (Session.Character.HasShopOpened || targetSession?.Character.HasShopOpened == true)
            {
                Session.CloseExchange(targetSession);
                return;
            }
            
            var packetsplit = packet.PacketData.Split(' ');
            if (packetsplit.Length < 2)
            {
                Session.SendPacket("exc_close 0");
                Session.CurrentMapInstance?.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone, "", Session.Character.ExchangeInfo.TargetCharacterId);

                if (targetSession != null) targetSession.Character.ExchangeInfo = null;
                Session.Character.ExchangeInfo = null;
                return;
            }

            var gold = packet.Gold;
            var bankGold = packet.BankGold;

            string packetList = "";

            if (gold < 0 || gold > Session.Character.Gold || Session.Character.ExchangeInfo == null
                || Session.Character.ExchangeInfo.ExchangeList.Count > 0)
            {
                return;
            }

            if (bankGold < 0 || bankGold > Session.Account.GoldBank / 1000 || Session.Character.ExchangeInfo == null
                || Session.Character.ExchangeInfo.ExchangeList.Count > 0)
            {
                return;
            }

            if (gold < 0 || gold > Session.Character.Gold || bankGold < 0 || bankGold > Session.Account.GoldBank / 1000 ||
                Session.Character.ExchangeInfo == null || Session.Character.ExchangeInfo.ExchangeList.Any())
            {
                return;
            }

            var exchangeList = packet.Data.MapToExchangeList().ToList();

            if (exchangeList.Any(s => s.Type == InventoryType.Bazaar) || exchangeList.Count > 10 || exchangeList.Count < 0)
            {
                Session.CloseExchange(targetSession);
                return;
            }

            var currentIndex = 0;
            foreach (var exchangeItem in exchangeList)
            {
                ItemInstance item = Session.Character.Inventory.LoadBySlotAndType(exchangeItem.Slot, exchangeItem.Type);
                if (item == null)
                {
                    return;
                }

                if (exchangeItem.Quantity <= 0 || item.Amount < exchangeItem.Quantity)
                {
                    return;
                }

                ItemInstance it = item.DeepCopy();
                if (it.Item.IsTradable && !it.IsBound)
                {
                    it.Amount = exchangeItem.Quantity;
                    Session.Character.ExchangeInfo.ExchangeList.Add(it);
                    if (exchangeItem.Type != InventoryType.Equipment)
                    {
                        packetList += $"{currentIndex}.{(byte)exchangeItem.Type}.{it.ItemVNum}.{exchangeItem.Quantity} ";
                    }
                    else
                    {
                        packetList += $"{currentIndex}.{(byte)exchangeItem.Type}.{it.ItemVNum}.{it.Rare}.{it.Upgrade} ";
                    }
                }
                else if (it.IsBound)
                {
                    Session.SendPacket("exc_close 0");
                    Session.CurrentMapInstance?.Broadcast(Session, "exc_close 0", ReceiverType.OnlySomeone,
                        "", Session.Character.ExchangeInfo.TargetCharacterId);

                    if (targetSession != null)
                    {
                        targetSession.Character.ExchangeInfo = null;
                    }
                    Session.Character.ExchangeInfo = null;
                    return;
                }

                currentIndex++;
            }

            Session.Character.ExchangeInfo.Gold = gold;
            Session.Character.ExchangeInfo.BankGold = bankGold * 1000;
            Session.CurrentMapInstance?.Broadcast(Session,
                $"exc_list 1 {Session.Character.CharacterId} {gold} {bankGold} {packetList}", ReceiverType.OnlySomeone,
                "", Session.Character.ExchangeInfo.TargetCharacterId);
            Session.Character.ExchangeInfo.Validated = true;
        }
    }
}
