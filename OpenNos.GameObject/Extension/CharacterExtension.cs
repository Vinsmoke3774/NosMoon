using System;
using System.Linq;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;

namespace OpenNos.GameObject.Extension
{
    public static class CharacterExtension
    {
        #region Methods

        public static void GoldLess(this ClientSession session, long value)
        {
            session.Character.Gold -= value;
            session.SendPacket(session.Character.GenerateGold());
        }

        public static void GoldUp(this ClientSession session, long value)
        {
            session.Character.Gold += value;
            session.SendPacket(session.Character.GenerateGold());
        }

        public static void OpenBank(this ClientSession Session)
        {
            Session.SendPacket(Session.Character.GenerateGb((byte)GoldBankPacketType.OpenBank));
            Session.SendPacket(UserInterfaceHelper.GenerateShopMemo((byte)SmemoType.Information, Language.Instance.GetMessageFromKey("OPEN_BANK")));
        }

        public static void SendShopEnd(this ClientSession s)
        {
            s.SendPacket("shop_end 2");
            s.SendPacket("shop_end 1");
        }
        #endregion
    }
}