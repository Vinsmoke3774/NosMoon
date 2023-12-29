using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenNos.Handler.SharedMethods
{

    public static class SharedInventoryMethods
    {

        public static void List(this ClientSession session, ClientSession targetSession)
        {
            if (targetSession == null)
            {
                return;
            }
            if (!session.Character.InExchangeOrTrade || !targetSession.Character.InExchangeOrTrade)
            {
                if (targetSession.Character.CharacterId == session.Character.CharacterId
                    || session.Character.Speed == 0 || targetSession == null
                    || targetSession.Character.TradeRequests.All(s => s != session.Character.CharacterId))
                {
                    return;
                }

                if (session.Character.InExchangeOrTrade || targetSession.Character.InExchangeOrTrade)
                {
                    foreach (var team in ServerManager.Instance.Sessions.Where(s => s.Account.Authority >= AuthorityType.GM))
                    {
                        if (team.HasSelectedCharacter)
                        {
                            team.SendPacket(team.Character.GenerateSay($"User {session.Character.Name} could be hacking. He sent a request to {targetSession.Character.Name} while having an open exchange.", 12));
                        }
                    }
                    return;
                }

                if (session.Character.Group != null
                    && session.Character.Group?.GroupType != GroupType.Group)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_RAID"), 0));
                    return;
                }

                if (targetSession.Character.Group != null
                    && targetSession.Character.Group?.GroupType != GroupType.Group)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_WITH_RAID_MEMBER"),
                        0));
                    return;
                }

                session.SendPacket($"exc_list 1 {targetSession.Character.CharacterId} -1");
                session.Character.ExchangeInfo = new ExchangeInfo
                {
                    TargetCharacterId = targetSession.Character.CharacterId,
                    Confirmed = false
                };
                targetSession.Character.ExchangeInfo = new ExchangeInfo
                {
                    TargetCharacterId = session.Character.CharacterId,
                    Confirmed = false
                };
                session.CurrentMapInstance?.Broadcast(session,
                    $"exc_list 1 {session.Character.CharacterId} -1", ReceiverType.OnlySomeone,
                    "", targetSession.Character.CharacterId);
            }
            else
            {
                session.CurrentMapInstance?.Broadcast(session,
                    UserInterfaceHelper.GenerateModal(
                        Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE"), 0),
                    ReceiverType.OnlySomeone, "", targetSession.Character.CharacterId);
            }
        }


        // Requesting Exchange
        public static void Request(this ClientSession session, ClientSession targetSession)
        {
            if (!session.HasCurrentMapInstance)
            {
                return;
            }

            if (targetSession?.Account == null)
            {
                return;
            }

            if (targetSession.CurrentMapInstance?.MapInstanceType == MapInstanceType.ArenaInstance)
            {
                session.SendPacket(UserInterfaceHelper.GenerateMsg(
                    Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_ARENA"),
                    0));
                return;
            }

            if (targetSession.CurrentMapInstance?.MapInstanceType == MapInstanceType.TalentArenaMapInstance)
            {
                session.SendPacket(UserInterfaceHelper.GenerateMsg(
                    Language.Instance.GetMessageFromKey("EXCHANGE_NOT_ALLOWED_IN_ARENA"),
                    0));
                return;
            }

            if (session.Character.IsBlockedByCharacter(targetSession.Character.CharacterId))
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(
                        Language.Instance.GetMessageFromKey("BLACKLIST_BLOCKED")));
                return;
            }

            if (session.Character.Speed == 0 || targetSession.Character.Speed == 0)
            {
                session.Character.ExchangeBlocked = true;
            }

            if (targetSession.Character.LastSkillUse.AddSeconds(20) > DateTime.Now
                || targetSession.Character.LastDefence.AddSeconds(20) > DateTime.Now)
            {
                session.SendPacket(UserInterfaceHelper.GenerateInfo(
                    string.Format(Language.Instance.GetMessageFromKey("PLAYER_IN_BATTLE"),
                        targetSession.Character.Name)));
                return;
            }

            if (session.Character.LastSkillUse.AddSeconds(20) > DateTime.Now
                || session.Character.LastDefence.AddSeconds(20) > DateTime.Now)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateInfo(Language.Instance.GetMessageFromKey("IN_BATTLE")));
                return;
            }

            if (session.Character.HasShopOpened || targetSession.Character.HasShopOpened)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateMsg(
                        Language.Instance.GetMessageFromKey("HAS_SHOP_OPENED"), 10));
                return;
            }

            if (targetSession.Character.ExchangeBlocked)
            {
                session.SendPacket(
                    session.Character.GenerateSay(Language.Instance.GetMessageFromKey("TRADE_BLOCKED"),
                        11));
                return;
            }

            if (session.Character.InExchangeOrTrade || targetSession.Character.InExchangeOrTrade)
            {
                session.SendPacket(
                    UserInterfaceHelper.GenerateModal(
                        Language.Instance.GetMessageFromKey("ALREADY_EXCHANGE"), 0));
                return;
            }

            session.SendPacket(UserInterfaceHelper.GenerateModal(
                string.Format(Language.Instance.GetMessageFromKey("YOU_ASK_FOR_EXCHANGE"),
                    targetSession.Character.Name), 0));

            Logger.Log.LogUserEvent("TRADE_REQUEST", session.GenerateIdentity(),
                $"[ExchangeRequest][{targetSession.GenerateIdentity()}]");

            session.Character.TradeRequests.Add(targetSession.Character.CharacterId);
            targetSession.SendPacket(UserInterfaceHelper.GenerateDialog($"#req_exc^2^{session.Character.CharacterId} #req_exc^5^{session.Character.CharacterId} {string.Format(Language.Instance.GetMessageFromKey("INCOMING_EXCHANGE"), session.Character.Name, session.Character.Class, session.Character.Level, session.Character.HeroLevel)}"));
        }

        // Close exchange
        public static void Close(this ClientSession session, ClientSession targetSession)
        {
            if (session.HasCurrentMapInstance && session.Character.ExchangeInfo != null)
            {
                targetSession =
                    session.CurrentMapInstance.GetSessionByCharacterId(session.Character.ExchangeInfo
                        .TargetCharacterId);
                CloseExchange(session, targetSession);
            }
        }

        // Confirm Exchange
        public static void Confirm(this ClientSession session)
        {
            if (!session.HasCurrentMapInstance || !session.HasSelectedCharacter ||
                session.Character.ExchangeInfo == null ||
                session.Character.ExchangeInfo.TargetCharacterId == session.Character.CharacterId)
            {
                return;
            }

            var targetSession = session.CurrentMapInstance.GetSessionByCharacterId(session.Character.ExchangeInfo.TargetCharacterId);

            if (targetSession == null)
            {
                return;
            }

            if (session.IsDisposing || targetSession.IsDisposing)
            {
                CloseExchange(session, targetSession);
                return;
            }

            ExchangeInfo targetExchange = targetSession.Character.ExchangeInfo;
            Inventory inventory = targetSession.Character.Inventory;

            long gold = targetSession.Character.Gold;
            long goldBank = targetSession.Account.GoldBank;
            long maxGold = ServerManager.Instance.Configuration.MaxGold;
            long maxGoldBank = ServerManager.Instance.Configuration.MaxGoldBank;

            if (targetExchange == null || session.Character.ExchangeInfo == null)
            {
                return;
            }

            if (session.Character.ExchangeInfo.Validated && targetExchange.Validated)
            {
                Logger.Log.LogUserEvent("TRADE_ACCEPT", session.GenerateIdentity(),
                    $"[ExchangeAccept][{targetSession.GenerateIdentity()}]");
                try
                {
                    session.Character.ExchangeInfo.Confirmed = true;
                    if (targetExchange.Confirmed
                        && session.Character.ExchangeInfo.Confirmed)
                    {
                        session.SendPacket("exc_close 1");
                        targetSession.SendPacket("exc_close 1");

                        bool continues = true;
                        bool goldmax = false;
                        if (!session.Character.Inventory.EnoughPlace(targetExchange
                            .ExchangeList))
                        {
                            continues = false;
                        }

                        continues &=
                            inventory.EnoughPlace(session.Character.ExchangeInfo
                                .ExchangeList);
                        goldmax |= session.Character.ExchangeInfo.Gold + gold > maxGold;
                        goldmax |= session.Character.ExchangeInfo.BankGold + goldBank > maxGoldBank;
                        if (session.Character.ExchangeInfo.Gold > session.Character.Gold || session.Character.ExchangeInfo.BankGold > session.Account.GoldBank)
                        {
                            return;
                        }

                        goldmax |= targetExchange.Gold + session.Character.Gold > maxGold;
                        goldmax |= targetExchange.BankGold + session.Account.GoldBank > maxGoldBank;
                        if (!continues || goldmax)
                        {
                            string message = !continues
                                ? UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("NOT_ENOUGH_PLACE"),
                                    0)
                                : UserInterfaceHelper.GenerateMsg(
                                    Language.Instance.GetMessageFromKey("MAX_GOLD"), 0);
                            session.SendPacket(message);
                            targetSession.SendPacket(message);
                            CloseExchange(session, targetSession);
                        }
                        else if (session.Character.Gold < session.Character.ExchangeInfo.Gold || targetSession.Character.Gold < targetExchange.Gold || session.Account.GoldBank < session.Character.ExchangeInfo.BankGold || targetSession.Account.GoldBank < targetExchange.BankGold)
                        {
                            string message = UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("ERROR_ON_EXANGE"), 0);
                            session.SendPacket(message);
                            targetSession.SendPacket(message);
                            CloseExchange(session, targetSession);
                        }
                        else
                        {
                            if (session.Character.ExchangeInfo.ExchangeList.Any(ei =>
                                !(ei.Item.IsTradable || ei.IsBound)))
                            {
                                session.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey(
                                            "ITEM_NOT_TRADABLE"), 0));
                                CloseExchange(session, targetSession);
                            }
                            if (targetSession.Character.ExchangeInfo.ExchangeList.Any(ei =>
                                !(ei.Item.IsTradable || ei.IsBound)))
                            {
                                targetSession.SendPacket(
                                    UserInterfaceHelper.GenerateMsg(
                                        Language.Instance.GetMessageFromKey(
                                            "ITEM_NOT_TRADABLE"), 0));
                                CloseExchange(targetSession, session);
                            }
                            else // all items can be traded
                            {
                                session.Character.IsExchanging =
                                    targetSession.Character.IsExchanging = true;

                                session.Character.LastExchange = DateTime.Now;
                                // exchange all items from source to target
                                session.Exchange(targetSession);

                                targetSession.Character.LastExchange = DateTime.Now;
                                // exchange all items from target to source
                                targetSession.Exchange(session);

                                session.Character.IsExchanging =
                                    targetSession.Character.IsExchanging = false;
                            }
                        }
                    }
                    else
                    {
                        session.SendPacket(UserInterfaceHelper.GenerateInfo(
                            string.Format(
                                Language.Instance.GetMessageFromKey("IN_WAITING_FOR"),
                                targetSession.Character.Name)));
                    }
                }
                catch (Exception nre)
                {
                    Logger.Log.Error(null, nre);
                }
            }
        }

        // Decline Exchange
        public static void Decline(this ClientSession session, ClientSession targetSession)
        {
            if (targetSession == null)
            {
                foreach (var team in ServerManager.Instance.Sessions.Where(s => s.Account.Authority >= AuthorityType.GM))
                {
                    if (team.HasSelectedCharacter)
                    {
                        team.SendPacket(team.Character.GenerateSay($"User {session.Character.Name} could be hacking. He sent an empty packet while trading.", 12));
                    }
                }
                return;
            }
            if (session.Character.InExchangeOrTrade || targetSession.Character.InExchangeOrTrade)
            {
                foreach (var team in ServerManager.Instance.Sessions.Where(s => s.Account.Authority >= AuthorityType.GM))
                {
                    if (team.HasSelectedCharacter)
                    {
                        team.SendPacket(team.Character.GenerateSay($"User {session.Character.Name} could be hacking. He declined a trade of  {targetSession.Character.Name} while he is trading with him.", 12));
                    }
                }
                CloseExchange(session, targetSession);
                return;
            }

            if (targetSession != null)
            {
                targetSession.Character.ExchangeInfo = null;
                targetSession.SendPacket(
                    session.Character.GenerateSay(
                        string.Format(Language.Instance.GetMessageFromKey("EXCHANGE_REFUSED"),
                            session.Character.Name), 10));
            }
            session.Character.ExchangeInfo = null;
            session.SendPacket(
                session.Character.GenerateSay(Language.Instance.GetMessageFromKey("YOU_REFUSED"), 10));
        }

        /// <summary>
        /// exchange closure method
        /// </summary>
        /// <param name="session"></param>
        /// <param name="targetSession"></param>
        public static void CloseExchange(this ClientSession session, ClientSession targetSession)
        {
            if (targetSession?.Character.ExchangeInfo != null)
            {
                targetSession.SendPacket("exc_close 0");
                targetSession.Character.ExchangeInfo = null;
            }

            if (session?.Character.ExchangeInfo != null)
            {
                session.SendPacket("exc_close 0");
                session.Character.ExchangeInfo = null;
            }
        }

        /// <summary>
        /// exchange initialization method
        /// </summary>
        /// <param name="sourceSession"></param>
        /// <param name="targetSession"></param>
        public static void Exchange(this ClientSession sourceSession, ClientSession targetSession)
        {
            try
            {
                if (sourceSession?.Character.ExchangeInfo == null)
                {
                    return;
                }

                string datalog = "";
                string data = "";
                Logger.Log.Debug($"[TRADE START] from {sourceSession.Character.Name} to {targetSession.Character.Name}");
                // remove all items from source session
                foreach (ItemInstance item in sourceSession.Character.ExchangeInfo.ExchangeList)
                {
                    ItemInstance invtemp = sourceSession.Character.Inventory.GetItemInstanceById(item.Id);
                    if (invtemp != null && invtemp?.Amount >= item.Amount)
                    {
                        sourceSession.Character.Inventory.RemoveItemFromInventory(invtemp.Id, item.Amount);
                    }
                    else
                    {
                        return;
                    }
                }

                // add all items to target session
                foreach (ItemInstance item in sourceSession.Character.ExchangeInfo.ExchangeList)
                {
                    ItemInstance item2 = item.DeepCopy();
                    item2.Id = Guid.NewGuid();
                    data +=
                        $"[OldIIId: {item.Id} NewIIId: {item2.Id} ItemVNum: {item.ItemVNum} Amount: {item.Amount} Rare: {item.Rare} Upgrade: {item.Upgrade}]";
                    datalog +=
                        $"[ItemVNum: {item.ItemVNum}] [Amount: {item.Amount}] [Rare: {item.Rare}] [Upgrade: {item.Upgrade}]";
                    Logger.Log.Debug(
                        $"[TRADE ACTION] Transfer item [ItemVNum: {item.ItemVNum}] [Amount: {item.Amount}] [Rare: {item.Rare}] [Upgrade: {item.Upgrade}] to {targetSession.Character.Name}");
                    List<ItemInstance> inv = targetSession.Character.Inventory.AddToInventory(item2);

                    if (inv.Count == 0)
                    {
                        // do what?
                    }
                }

                data += $"[Gold: {sourceSession.Character.ExchangeInfo.Gold}]";
                data += $"[BankGold: {sourceSession.Character.ExchangeInfo.BankGold}]";
                datalog += "\n" + $"[Gold: {sourceSession.Character.ExchangeInfo.Gold}]";
                datalog += $"[BankGold: {sourceSession.Character.ExchangeInfo.BankGold}]";

                // handle gold
                sourceSession.Character.Gold -= sourceSession.Character.ExchangeInfo.Gold;
                sourceSession.Account.GoldBank -= sourceSession.Character.ExchangeInfo.BankGold;
                sourceSession.SendPacket(sourceSession.Character.GenerateGold());

                targetSession.Character.Gold += sourceSession.Character.ExchangeInfo.Gold;
                targetSession.Account.GoldBank += sourceSession.Character.ExchangeInfo.BankGold;
                targetSession.SendPacket(targetSession.Character.GenerateGold());

                // all items and gold from sourceSession have been transferred, clean exchange info

                Logger.Log.Debug($"[TRADE_COMPLETE] from {sourceSession.Character.Name} to {targetSession.Character.Name}");
                Logger.Log.LogUserEvent("TRADE_COMPLETE", sourceSession.GenerateIdentity(),
                    $"[{targetSession.GenerateIdentity()}]Data: {data}");


                sourceSession.Character.ExchangeInfo = null;
            }
            catch (Exception e)
            {
                try
                {
                    Logger.Log.Error(null, e);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// changesp private method
        /// </summary>
        public static void LoadSPStats(this ClientSession session)
        {
            ItemInstance sp =
                session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
            ItemInstance fairy =
                session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (sp != null)
            {
                ItemInstance mainWeapon = session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.MainWeapon, InventoryType.Wear);
                ItemInstance secondaryWeapon = session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.SecondaryWeapon, InventoryType.Wear);
                
                List<ShellEffectDTO> effects = new List<ShellEffectDTO>();
                
                if (mainWeapon?.ShellEffects != null)
                {
                    effects.AddRange(mainWeapon.ShellEffects);
                }

                if (secondaryWeapon?.ShellEffects != null)
                {
                    effects.AddRange(secondaryWeapon.ShellEffects);
                }

                int GetShellWeaponEffectValue(ShellWeaponEffectType effectType)
                {
                    return effects.Where(s => s.Effect == (byte)effectType).OrderByDescending(s => s.Value)
                               .FirstOrDefault()?.Value ?? 0;
                }

                int slElement = CharacterHelper.SlPoint(sp.SlElement, 2)
                                + GetShellWeaponEffectValue(ShellWeaponEffectType.SLElement)
                                + GetShellWeaponEffectValue(ShellWeaponEffectType.SLGlobal);
                int slHp = CharacterHelper.SlPoint(sp.SlHP, 3)
                           + GetShellWeaponEffectValue(ShellWeaponEffectType.SLHP)
                           + GetShellWeaponEffectValue(ShellWeaponEffectType.SLGlobal);
                int slDefence = CharacterHelper.SlPoint(sp.SlDefence, 1)
                                + GetShellWeaponEffectValue(ShellWeaponEffectType.SLDefence)
                                + GetShellWeaponEffectValue(ShellWeaponEffectType.SLGlobal);
                int slHit = CharacterHelper.SlPoint(sp.SlDamage, 0)
                            + GetShellWeaponEffectValue(ShellWeaponEffectType.SLDamage)
                            + GetShellWeaponEffectValue(ShellWeaponEffectType.SLGlobal);

                #region slHit

                sp.DamageMinimum = 0;
                sp.DamageMaximum = 0;
                sp.HitRate = 0;
                sp.CriticalLuckRate = 0;
                sp.CriticalRate = 0;
                sp.DefenceDodge = 0;
                sp.DistanceDefenceDodge = 0;
                sp.ElementRate = 0;
                sp.DarkResistance = 0;
                sp.LightResistance = 0;
                sp.FireResistance = 0;
                sp.WaterResistance = 0;
                sp.CriticalDodge = 0;
                sp.CloseDefence = 0;
                sp.DistanceDefence = 0;
                sp.MagicDefence = 0;
                sp.HP = 0;
                sp.MP = 0;

                if (slHit >= 1)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                }

                if (slHit >= 10)
                {
                    sp.HitRate += 10;
                }

                if (slHit >= 20)
                {
                    sp.CriticalLuckRate += 2;
                }

                if (slHit >= 30)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                    sp.HitRate += 10;
                }

                if (slHit >= 40)
                {
                    sp.CriticalRate += 10;
                }

                if (slHit >= 50)
                {
                    sp.HP += 200;
                    sp.MP += 200;
                }

                if (slHit >= 60)
                {
                    sp.HitRate += 15;
                }

                if (slHit >= 70)
                {
                    sp.HitRate += 15;
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                }

                if (slHit >= 80)
                {
                    sp.CriticalLuckRate += 3;
                }

                if (slHit >= 90)
                {
                    sp.CriticalRate += 20;
                }

                if (slHit >= 100)
                {
                    sp.CriticalLuckRate += 3;
                    sp.CriticalRate += 20;
                    sp.HP += 200;
                    sp.MP += 200;
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                    sp.HitRate += 20;
                }

                #endregion

                #region slDefence

                if (slDefence >= 10)
                {
                    sp.DefenceDodge += 5;
                    sp.DistanceDefenceDodge += 5;
                }

                if (slDefence >= 20)
                {
                    sp.CriticalDodge += 2;
                }

                if (slDefence >= 30)
                {
                    sp.HP += 100;
                }

                if (slDefence >= 40)
                {
                    sp.CriticalDodge += 2;
                }

                if (slDefence >= 50)
                {
                    sp.DefenceDodge += 5;
                    sp.DistanceDefenceDodge += 5;
                }

                if (slDefence >= 60)
                {
                    sp.HP += 200;
                }

                if (slDefence >= 70)
                {
                    sp.CriticalDodge += 3;
                }

                if (slDefence >= 75)
                {
                    sp.FireResistance += 2;
                    sp.WaterResistance += 2;
                    sp.LightResistance += 2;
                    sp.DarkResistance += 2;
                }

                if (slDefence >= 80)
                {
                    sp.DefenceDodge += 10;
                    sp.DistanceDefenceDodge += 10;
                    sp.CriticalDodge += 3;
                }

                if (slDefence >= 90)
                {
                    sp.FireResistance += 3;
                    sp.WaterResistance += 3;
                    sp.LightResistance += 3;
                    sp.DarkResistance += 3;
                }

                if (slDefence >= 95)
                {
                    sp.HP += 300;
                }

                if (slDefence >= 100)
                {
                    sp.DefenceDodge += 20;
                    sp.DistanceDefenceDodge += 20;
                    sp.FireResistance += 5;
                    sp.WaterResistance += 5;
                    sp.LightResistance += 5;
                    sp.DarkResistance += 5;
                }

                #endregion

                #region slHp

                if (slHp >= 5)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                }

                if (slHp >= 10)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                }

                if (slHp >= 15)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                }

                if (slHp >= 20)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                    sp.CloseDefence += 10;
                    sp.DistanceDefence += 10;
                    sp.MagicDefence += 10;
                }

                if (slHp >= 25)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                }

                if (slHp >= 30)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                }

                if (slHp >= 35)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                }

                if (slHp >= 40)
                {
                    sp.DamageMinimum += 5;
                    sp.DamageMaximum += 5;
                    sp.CloseDefence += 15;
                    sp.DistanceDefence += 15;
                    sp.MagicDefence += 15;
                }

                if (slHp >= 45)
                {
                    sp.DamageMinimum += 10;
                    sp.DamageMaximum += 10;
                }

                if (slHp >= 50)
                {
                    sp.DamageMinimum += 10;
                    sp.DamageMaximum += 10;
                    sp.FireResistance += 2;
                    sp.WaterResistance += 2;
                    sp.LightResistance += 2;
                    sp.DarkResistance += 2;
                }

                if (slHp >= 55)
                {
                    sp.DamageMinimum += 10;
                    sp.DamageMaximum += 10;
                }

                if (slHp >= 60)
                {
                    sp.DamageMinimum += 10;
                    sp.DamageMaximum += 10;
                }

                if (slHp >= 65)
                {
                    sp.DamageMinimum += 10;
                    sp.DamageMaximum += 10;
                }

                if (slHp >= 70)
                {
                    sp.DamageMinimum += 10;
                    sp.DamageMaximum += 10;
                    sp.CloseDefence += 20;
                    sp.DistanceDefence += 20;
                    sp.MagicDefence += 20;
                }

                if (slHp >= 75)
                {
                    sp.DamageMinimum += 15;
                    sp.DamageMaximum += 15;
                }

                if (slHp >= 80)
                {
                    sp.DamageMinimum += 15;
                    sp.DamageMaximum += 15;
                }

                if (slHp >= 85)
                {
                    sp.DamageMinimum += 15;
                    sp.DamageMaximum += 15;
                    sp.CriticalDodge++;
                }

                if (slHp >= 86)
                {
                    sp.CriticalDodge++;
                }

                if (slHp >= 87)
                {
                    sp.CriticalDodge++;
                }

                if (slHp >= 88)
                {
                    sp.CriticalDodge++;
                }

                if (slHp >= 90)
                {
                    sp.DamageMinimum += 15;
                    sp.DamageMaximum += 15;
                    sp.CloseDefence += 25;
                    sp.DistanceDefence += 25;
                    sp.MagicDefence += 25;
                }

                if (slHp >= 91)
                {
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 92)
                {
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 93)
                {
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 94)
                {
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 95)
                {
                    sp.DamageMinimum += 20;
                    sp.DamageMaximum += 20;
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 96)
                {
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 97)
                {
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 98)
                {
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 99)
                {
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                }

                if (slHp >= 100)
                {
                    sp.FireResistance += 3;
                    sp.WaterResistance += 3;
                    sp.LightResistance += 3;
                    sp.DarkResistance += 3;
                    sp.CloseDefence += 30;
                    sp.DistanceDefence += 30;
                    sp.MagicDefence += 30;
                    sp.DamageMinimum += 20;
                    sp.DamageMaximum += 20;
                    sp.DefenceDodge += 2;
                    sp.DistanceDefenceDodge += 2;
                    sp.CriticalDodge++;
                }

                #endregion

                #region slElement

                if (slElement >= 1)
                {
                    sp.ElementRate += 2;
                }

                if (slElement >= 10)
                {
                    sp.MP += 100;
                }

                if (slElement >= 20)
                {
                    sp.MagicDefence += 5;
                }

                if (slElement >= 30)
                {
                    sp.FireResistance += 2;
                    sp.WaterResistance += 2;
                    sp.LightResistance += 2;
                    sp.DarkResistance += 2;
                    sp.ElementRate += 2;
                }

                if (slElement >= 40)
                {
                    sp.MP += 100;
                }

                if (slElement >= 50)
                {
                    sp.MagicDefence += 5;
                }

                if (slElement >= 60)
                {
                    sp.FireResistance += 3;
                    sp.WaterResistance += 3;
                    sp.LightResistance += 3;
                    sp.DarkResistance += 3;
                    sp.ElementRate += 2;
                }

                if (slElement >= 70)
                {
                    sp.MP += 100;
                }

                if (slElement >= 80)
                {
                    sp.MagicDefence += 5;
                }

                if (slElement >= 90)
                {
                    sp.FireResistance += 4;
                    sp.WaterResistance += 4;
                    sp.LightResistance += 4;
                    sp.DarkResistance += 4;
                    sp.ElementRate += 2;
                }

                if (slElement >= 100)
                {
                    sp.FireResistance += 6;
                    sp.WaterResistance += 6;
                    sp.LightResistance += 6;
                    sp.DarkResistance += 6;
                    sp.MagicDefence += 5;
                    sp.MP += 200;
                    sp.ElementRate += 2;
                }

                #endregion

                session.SendPackets(session.Character.GenerateStatChar());
                session.SendPacket(session.Character.GenerateStat());
            }
        }

        /// <summary>
        /// changesp private method
        /// </summary>
        public static void ChangeSp(this ClientSession session)
        {
            ItemInstance sp = session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Sp, InventoryType.Wear);
            ItemInstance fairy = session.Character.Inventory.LoadBySlotAndType((byte)EquipmentType.Fairy, InventoryType.Wear);
            if (sp != null)
            {
                if (session.Character.GetReputationIco() < sp.Item.ReputationMinimum)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("LOW_REP"), 0));
                    return;
                }

                if (fairy != null && sp.Item.Element != 0 && fairy.Item.Element != sp.Item.Element && fairy.Item.Element != sp.Item.SecondaryElement)
                {
                    session.SendPacket(UserInterfaceHelper.GenerateMsg(Language.Instance.GetMessageFromKey("BAD_FAIRY"), 0));
                    return;
                }

                if (new int[] { 4494, 4495, 4496 }.Contains(sp.ItemVNum))
                {
                    if (session.Character.Timespace == null)
                    {
                        return;
                    }
                    else if (ServerManager.Instance.TimeSpaces.Any(s => s.SpNeeded?[(byte)session.Character.Class] == sp.ItemVNum))
                    {
                        if (session.Character.Timespace.SpNeeded?[(byte)session.Character.Class] != sp.ItemVNum)
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                session.Character.ChargeValue = 0;
                session.Character.DisableBuffs(BuffType.All);
                session.Character.EquipmentBCards.AddRange(sp.Item.BCards);
                session.Character.LastTransform = DateTime.Now;
                session.Character.UseSp = true;
                session.Character.Morph = sp.Item.Morph;
                session.Character.MorphUpgrade = sp.Upgrade;
                session.Character.MorphUpgrade2 = sp.Design;
                session.CurrentMapInstance?.Broadcast(session.Character.GenerateCMode());
                session.SendPacket(session.Character.GenerateLev());
                session.CurrentMapInstance?.Broadcast(
                    StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 196),
                    session.Character.PositionX, session.Character.PositionY);
                session.CurrentMapInstance?.Broadcast(
                    UserInterfaceHelper.GenerateGuri(6, 1, session.Character.CharacterId), session.Character.PositionX,
                    session.Character.PositionY);
                session.SendPacket(session.Character.GenerateSpPoint());
                session.Character.LoadSpeed();
                session.SendPacket(session.Character.GenerateCond());
                if (session.Character.IsInArenaLobby)
                {
                    session.Character.Hp = (int)session.Character?.HPLoad();
                    session.Character.Mp = (int)session.Character?.MPLoad();
                }
                session.SendPacket(session.Character.GenerateStat());
                session.SendPackets(session.Character.GenerateStatChar());
                Group grp = session.Character?.Group;

                if (grp != null && grp.GroupType >= GroupType.Team)
                {
                    grp.Sessions.ForEach(s =>
                    {
                        s.SendPacket(grp.GenerateRdlst());
                    });
                }

                session.Character.SkillsSp = new ThreadSafeSortedList<int, CharacterSkill>();
                Parallel.ForEach(ServerManager.GetAllSkill(), skill =>
                {
                    if (skill.UpgradeType == sp.Item.Morph && skill.SkillType == 1 && sp.SpLevel >= skill.LevelMinimum)
                    {
                        session.Character.SkillsSp[skill.SkillVNum] = new CharacterSkill
                        {
                            SkillVNum = skill.SkillVNum,
                            CharacterId = session.Character.CharacterId
                        };
                    }
                });
                session.SendPacket(session.Character.GenerateSki());
                session.SendPackets(session.Character.GenerateQuicklist());
                Logger.Log.LogUserEvent("CHARACTER_SPECIALIST_CHANGE", session.GenerateIdentity(), $"Specialist: {sp.Item.Morph}");
            }
        }

    }
}
