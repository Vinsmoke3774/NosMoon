using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Networking;

namespace OpenNos.Handler.World.ScriptedInstance
{
    public class GetGiftPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public GetGiftPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// RSelPacket packet
        /// </summary>
        /// <param name="rSelPacket"></param>
        public void GetGift(RSelPacket rSelPacket)
        {
            if (Session.Character.Timespace?.FirstMap?.MapInstanceType == MapInstanceType.TimeSpaceInstance)
            {
                ServerManager.GetBaseMapInstanceIdByMapId(Session.Character.MapId);
                if (Session.Character.Timespace?.FirstMap.InstanceBag.EndState == 5 || Session.Character.Timespace?.FirstMap.InstanceBag.EndState == 6)
                {
                    if (!Session.Character.TimespaceRewardGotten)
                    {
                        Session.Character.TimespaceRewardGotten = true;

                        if (Session.Character.Timespace.IsLoserMode)
                        {
                            Session.Character.Timespace.Reputation = 0;
                            Session.Character.Timespace.Gold = 0;
                            Session.Character.Timespace.DrawItems = new();
                            Session.Character.Timespace.GiftItems = new();
                            Session.Character.Timespace.SpecialItems = new();
                        }

                        Session.Character.GetReputation(Session.Character.Timespace.Reputation);

                        Session.Character.Gold =
                            Session.Character.Gold + Session.Character.Timespace.Gold
                            > ServerManager.Instance.Configuration.MaxGold
                                ? ServerManager.Instance.Configuration.MaxGold
                                : Session.Character.Gold + Session.Character.Timespace.Gold;
                        Session.SendPacket(Session.Character.GenerateGold());
                        Session.SendPacket(Session.Character.GenerateSay(
                            string.Format(Language.Instance.GetMessageFromKey("GOLD_TS_END"),
                                Session.Character.Timespace.Gold), 10));

                        int rand = new Random().Next(Session.Character.Timespace.DrawItems.Count);
                        string repay = "repay ";
                        if (Session.Character.Timespace.DrawItems.Count > 0)
                        {
                            Session.Character.GiftAdd(Session.Character.Timespace.DrawItems[rand].VNum,
                                Session.Character.Timespace.DrawItems[rand].Amount,
                                design: Session.Character.Timespace.DrawItems[rand].Design,
                                forceRandom: Session.Character.Timespace.DrawItems[rand].IsRandomRare);
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            Gift gift = Session.Character.Timespace.GiftItems.ElementAtOrDefault(i);
                            repay += gift == null ? "-1.0.0 " : $"{gift.VNum}.0.{gift.Amount} ";
                            if (gift != null)
                            {
                                Session.Character.GiftAdd(gift.VNum, gift.Amount, design: gift.Design, forceRandom: gift.IsRandomRare);
                            }
                        }

                        // TODO: Add HasAlreadyDone
                        for (int i = 0; i < 2; i++)
                        {
                            Gift gift = Session.Character.Timespace.SpecialItems.ElementAtOrDefault(i);
                            repay += gift == null ? "-1.0.0 " : $"{gift.VNum}.0.{gift.Amount} ";
                            if (gift != null)
                            {
                                Session.Character.GiftAdd(gift.VNum, gift.Amount, design: gift.Design, forceRandom: gift.IsRandomRare);
                            }
                        }

                        if (Session.Character.Timespace.DrawItems.Count > 0)
                        {
                            repay +=
                                $"{Session.Character.Timespace.DrawItems[rand].VNum}.0.{Session.Character.Timespace.DrawItems[rand].Amount}";
                        }
                        else
                        {
                            repay +=
                                $"-1.0.0";
                        }
                        Session.SendPacket(repay);
                        Session.Character.Timespace.FirstMap.InstanceBag.EndState = 6;
                    }
                }
            }
            if (Session.Character.SkyTower?.FirstMap?.MapInstanceType == MapInstanceType.SkyTowerInstance)
            {
                ServerManager.GetBaseMapInstanceIdByMapId(Session.Character.MapId);
                if (Session.Character.SkyTower?.FirstMap.InstanceBag.EndState == 5 || Session.Character.SkyTower?.FirstMap.InstanceBag.EndState == 6)
                {
                    Session.Character.GetReputation(Session.Character.SkyTower.Reputation);
                    Session.Character.Gold = Session.Character.Gold + Session.Character.SkyTower.Gold > ServerManager.Instance.Configuration.MaxGold ? ServerManager.Instance.Configuration.MaxGold : Session.Character.Gold + Session.Character.SkyTower.Gold;
                    Session.SendPacket(Session.Character.GenerateGold());
                    Session.SendPacket(Session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("GOLD_TS_END"), Session.Character.SkyTower.Gold), 10));

                    int rands = new Random().Next(Session.Character.SkyTower.DrawItems.Count);
                    string repays = "repay ";
                    if (Session.Character.SkyTower.DrawItems.Count > 0)
                    {
                        Session.Character.GiftAdd(Session.Character.SkyTower.DrawItems[rands].VNum,
                            Session.Character.SkyTower.DrawItems[rands].Amount,
                            design: Session.Character.SkyTower.DrawItems[rands].Design,
                            forceRandom: Session.Character.SkyTower.DrawItems[rands].IsRandomRare);
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        Gift gift = Session.Character.SkyTower.GiftItems.ElementAtOrDefault(i);
                        repays += gift == null ? "-1.0.0 " : $"{gift.VNum}.0.{gift.Amount} ";
                        if (gift != null)
                        {
                            Session.Character.GiftAdd(gift.VNum, gift.Amount, design: gift.Design, forceRandom: gift.IsRandomRare);
                        }
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        Gift gift = Session.Character.SkyTower.SpecialItems.ElementAtOrDefault(i);
                        repays += gift == null ? "-1.0.0 " : $"{gift.VNum}.0.{gift.Amount} ";
                        if (gift != null)
                        {
                            Session.Character.GiftAdd(gift.VNum, gift.Amount, design: gift.Design, forceRandom: gift.IsRandomRare);
                        }
                    }

                    if (Session.Character.SkyTower.DrawItems.Count > 0)
                    {
                        repays +=
                            $"{Session.Character.SkyTower.DrawItems[rands].VNum}.0.{Session.Character.SkyTower.DrawItems[rands].Amount}";
                    }
                    else
                    {
                        repays +=
                            $"-1.0.0";
                    }

                    Session.SendPacket(repays);
                    Session.Character.SkyTower.FirstMap.InstanceBag.EndState = 6;

                }
            }
        }
    }
}
