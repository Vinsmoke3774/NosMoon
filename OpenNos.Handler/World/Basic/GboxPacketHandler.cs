using System;
using System.Linq;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.Basic
{
    public class GboxPacketHandler : IPacketHandler
    {
        #region Instantiation

        public GboxPacketHandler(ClientSession session) => Session = session;

        #endregion

        #region Properties

        public ClientSession Session { get; }

        #endregion

        #region Methods

        public void BankAction(GboxPacket gboxPacket)
        {
            if (Session.Character.HasShopOpened ||
                !Session.HasCurrentMapInstance ||
                Session.Character.IsExchanging ||
                Session.Character.ExchangeInfo != null)
            {
                return;
            }

            if (Session.Character.InExchangeOrTrade)
            {
                return;
            }

            if (gboxPacket.Amount <= 0)
            {
                //Packet hacking duplication

                return;
            }

            if (Session.Account.GoldBank < 0)
            {
                // It kinda seems to happen sometimes for some reason ? It's fucking bullshit, but it should do the trick
                Session.Account.GoldBank = Math.Abs(Session.Account.GoldBank);
            }

            if (Session.Account.GoldBank > ServerManager.Instance.Configuration.MaxGoldBank)
            {
                Session.Account.GoldBank = ServerManager.Instance.Configuration.MaxGoldBank;
                return;
            }

            switch (gboxPacket.Type)
            {
                case BankActionType.Deposit:
                    if (gboxPacket.Option == 0)
                    {
                        Session.SendPacket($"qna #gbox^1^{gboxPacket.Amount}^1 Want to deposit {gboxPacket.Amount}000 gold?");
                        return;
                    }

                    if (gboxPacket.Option == 1)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Information, string.Format(Language.Instance.GetMessageFromKey("BANK_DEPOSIT"), Session.Account.GoldBank, Session.Character.Gold, gboxPacket.Amount * 1000)));
                        if (Session.Account.GoldBank + gboxPacket.Amount * 1000 > ServerManager.Instance.Configuration.MaxGoldBank)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("MAX_GOLD_BANK_REACHED")));
                            Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Error, Language.Instance.GetMessageFromKey("MAX_GOLD_BANK_REACHED")));
                            return;
                        }

                        if (Session.Character.Gold < gboxPacket.Amount * 1000)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("NOT_ENOUGH_GOLD")));
                            Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Error, Language.Instance.GetMessageFromKey("NOT_ENOUGH_GOLD")));
                            return;
                        }

                        Session.Account.GoldBank += gboxPacket.Amount * 1000;
                        Session.Character.Gold -= gboxPacket.Amount * 1000;
                        Session.SendPacket(Session.Character.GenerateGold());
                        Session.SendPacket(Session.Character.GenerateGb((byte)GoldBankPacketType.Deposit));
                        Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Balance, string.Format(Language.Instance.GetMessageFromKey("BANK_BALANCE"), Session.Account.GoldBank, Session.Character.Gold)));
                    }
                    break;

                case BankActionType.Withdraw:
                    if (gboxPacket.Option == 0)
                    {
                        Session.SendPacket($"qna #gbox^2^{gboxPacket.Amount}^1 Would you like to withdraw {gboxPacket.Amount}000 gold? (Fee: 0 gold)");
                        return;
                    }

                    if (gboxPacket.Option == 1)
                    {
                        Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Information, string.Format(Language.Instance.GetMessageFromKey("WITHDRAW_BANK"), Session.Account.GoldBank, Session.Character.Gold, gboxPacket.Amount * 1000)));
                        if (Session.Character.Gold + gboxPacket.Amount * 1000 > ServerManager.Instance.Configuration.MaxGold)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("TOO_MUCH_GOLD")));
                            Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Error, Language.Instance.GetMessageFromKey("TOO_MUCH_GOLD")));
                            return;
                        }

                        if (Session.Account.GoldBank < gboxPacket.Amount * 1000)
                        {
                            Session.SendPacket(UserInterfaceHelper.GenerateInfo("NOT_ENOUGH_FUNDS"));
                            Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Error, Language.Instance.GetMessageFromKey("NOT_ENOUGH_FUNDS")));
                            return;
                        }

                        Session.Account.GoldBank -= gboxPacket.Amount * 1000;
                        Session.Character.Gold += gboxPacket.Amount * 1000;
                        Session.SendPacket(Session.Character.GenerateGold());
                        Session.SendPacket(Session.Character.GenerateGb((byte)GoldBankPacketType.Withdraw));
                        Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Balance, string.Format(Language.Instance.GetMessageFromKey("BANK_BALANCE"), Session.Account.GoldBank, Session.Character.Gold)));
                    }
                    break;
            }
        }

        #endregion
    }
}