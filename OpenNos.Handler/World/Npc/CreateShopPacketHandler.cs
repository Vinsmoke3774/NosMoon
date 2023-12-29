using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets.Npc;
using OpenNos.Core;
using OpenNos.Core.Extensions;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;

namespace OpenNos.Handler.World.Npc
{
    public class CreateShopPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public CreateShopPacketHandler(ClientSession session) => Session = session;

        public void CreateShop(MShopPacket packet)
        {
            string shopname = "";
            if ((Session.Character.HasShopOpened && packet.Type != MShopType.CloseShop) || !Session.HasCurrentMapInstance
                                                                                        || Session.Character.IsExchanging ||
                                                                                        Session.Character.ExchangeInfo != null)
            {
                return;
            }

            if (Session.CurrentMapInstance.Portals.Any(por =>
                Session.Character.PositionX < por.SourceX + 6 && Session.Character.PositionX > por.SourceX - 6
                                                              && Session.Character.PositionY < por.SourceY + 6 &&
                                                              Session.Character.PositionY > por.SourceY - 6))
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NEAR_PORTAL"), 0));
                return;
            }

            if (Session.Character.Group != null && Session.Character.Group?.GroupType != GroupType.Group)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NOT_ALLOWED_IN_RAID"),
                        0));
                return;
            }

            if (!Session.CurrentMapInstance.ShopAllowed)
            {
                Session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("SHOP_NOT_ALLOWED"), 0));
                return;
            }


            switch (packet.Type)
            {
                case MShopType.OpenShop:
                    Session.SendPacket("ishop");
                    break;

                case MShopType.CreateNewShop:
                    if (Session.CurrentMapInstance.UserShops.Any(s =>
                        s.Value.OwnerId == Session.Character.CharacterId))
                    {
                        return;
                    }

                    MapShop myShop = new MapShop();
                    var mapped = packet.Data.MapToPersonnalShopItem(Session).ToList();

                    if (mapped.Count > 80)
                    {
                        return;
                    }

                    foreach (var shopItem in mapped)
                    {
                        var isAmountCorrect =
                            myShop.Items.Where(s => s.ItemInstance.ItemVNum == shopItem.ItemInstance.ItemVNum)
                                .Sum(s => s.SellAmount) + shopItem.SellAmount <=
                            Session.Character.Inventory.CountItem(shopItem.ItemInstance.ItemVNum);

                        if (!isAmountCorrect)
                        {
                            return;
                        }

                        myShop.Items.Add(shopItem);
                    }

                    if (myShop.Items.Count != 0)
                    {
                        if (!myShop.Items.Any(s => !s.ItemInstance.Item.IsSoldable || s.ItemInstance.IsBound))
                        {
                            var packetsplit = packet.Data.Split(' ');
                            for (int i = 80; i < packetsplit.Length; i++)
                            {
                                shopname += $"{packetsplit[i]} ";
                            }

                            // trim shopname
                            shopname = shopname.TrimEnd(' ');

                            // create default shopname if it's empty
                            if (string.IsNullOrWhiteSpace(shopname) || string.IsNullOrEmpty(shopname))
                            {
                                shopname = Language.Instance.GetMessageFromKey("SHOP_PRIVATE_SHOP");
                            }

                            // truncate the string to a max-length of 20
                            shopname = shopname.Truncate(20);
                            myShop.OwnerId = Session.Character.CharacterId;
                            myShop.Name = shopname;
                            Session.CurrentMapInstance.UserShops.Add(Session.CurrentMapInstance.LastUserShopId++,
                                myShop);

                            Session.Character.HasShopOpened = true;

                            Session.CurrentMapInstance?.Broadcast(Session,
                                Session.Character.GeneratePlayerFlag(Session.CurrentMapInstance.LastUserShopId),
                                ReceiverType.AllExceptMe);
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateShop(shopname));
                            Session.SendPacket(
                                UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("SHOP_OPEN")));

                            Session.Character.IsSitting = true;
                            Session.Character.IsShopping = true;

                            Session.Character.LoadSpeed();
                            Session.SendPacket(Session.Character.GenerateCond());
                            Session.CurrentMapInstance?.Broadcast(Session.Character.GenerateRest());
                        }
                        else
                        {
                            Session.SendPacket("shop_end 0");
                            Session.SendPacket(
                                Session.Character.GenerateSay(
                                    Language.Instance.GetMessageFromKey("ITEM_NOT_SOLDABLE"), 10));
                        }
                    }
                    else
                    {
                        Session.SendPacket("shop_end 0");
                        Session.SendPacket(
                            Session.Character.GenerateSay(Language.Instance.GetMessageFromKey("SHOP_EMPTY"), 10));
                    }

                    break;

                case MShopType.CloseShop:
                    Session.Character.CloseShop();
                    break;
            }
        }
    }
}
