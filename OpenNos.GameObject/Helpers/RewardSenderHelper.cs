using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NosByte.Shared.ApiModels;
using OpenNos.Core;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Extension;
using OpenNos.GameObject.Extension.CharacterExtensions;
using OpenNos.GameObject.HttpClients;
using OpenNos.GameObject.Networking;
using OpenNos.Master.Library.Client;
using OpenNos.Master.Library.Data;

namespace OpenNos.GameObject.Helpers
{
    //Not a singleton.
    public class RewardSenderHelper
    {
        public async Task SendInstantBattleWaveRewards(ClientSession session, InstantBattleWaveReward itm)
        {
            if (session == null)
            {
                return;
            }

            var onlineCharacters = CommunicationServiceClient.Instance.RetrieveOnlineCharacters(session.Character.CharacterId);

            if (onlineCharacters.Length > 1)
            {
                var uploadList = onlineCharacters.Select(online => online[0]).ToList();

                var result = UserHttpClient.Instance.GetCharacterListEvents(uploadList);

                if (result != null)
                {
                    result = result.Where(s => s.EventType == EventType.INSTANTBATTLE).OrderByDescending(s => s.LevelSum)?.ThenBy(s => s.CharacterId);
                    var selectedCharacter = result.FirstOrDefault();

                    if (selectedCharacter == null)
                    {
                        return;
                    }

                    if (session.Character.CharacterId != selectedCharacter.CharacterId)
                    {
                        session.SendPacket(session.Character.GenerateSay($"Character {selectedCharacter.Name} received rewards.", 11));
                        return;
                    }
                }
            }

            // Fuck you :)
            short[] allowedVnums = { 946, 945, 1436, 1438, 1437, 2014, 2018, 1439, 2112, 2113, 2114, 2115, 416, 2104 };

            var monstersOnMap = session.CurrentMapInstance?.Monsters?.Where(s => s != null && s.IsAlive && !s.IsMeteorite);

            bool sendReward = true;

            if (monstersOnMap != null)
            {
                if (monstersOnMap.Any())
                {
                    if (monstersOnMap.Select(monster => allowedVnums.Any(s => s == monster.MonsterVNum)).Any(isAllowed => !isAllowed))
                    {
                        sendReward = false;
                    }
                }
            }

            if (session.HasCurrentMapInstance &&  !sendReward)
            {
                session.SendPacket(session.Character.GenerateSay("You didn't clean the map, you don't get any rewards...", 11));
                return;
            }

            session.SendPacket(session.Character.GenerateSay($"The rewards for the wave {itm.RewardLevel} will be sent into your inventory !", 11));

            if (session.Character.EffectFromTitle == null)
            {
                session.Character.EffectFromTitle = new ThreadSafeGenericList<BCard>();
            }

            var hasEndlessBattleTitle = session.Character.EffectFromTitle.Any(s => s.Type == (byte)BCardType.CardType.BonusBCards && s.SubType == (byte)AdditionalTypes.BonusTitleBCards.InstantBattleAFKPower);

            var totalGold = (int) (itm.Gold * (hasEndlessBattleTitle ? 1.5 : 1));
            session.Character.GetGold(totalGold);

            foreach (var reward in itm.Items)
            {
                var totalAmount = (int) (reward.Amount * (hasEndlessBattleTitle ? 1.5 : 1));
                session.Character.GiftAdd((short)reward.VNum, (short)totalAmount);
                await Task.Delay(100);
            }
        }

        public void SendInstantBattleEndRewards(ClientSession session, Tuple<MapInstance, byte> mapInstance)
        {
            if (session?.Character == null)
            {
                return;
            }

            var onlineCharacters = CommunicationServiceClient.Instance.RetrieveOnlineCharacters(session.Character.CharacterId);

            if (onlineCharacters.Length > 1)
            {
                var uploadList = onlineCharacters.Select(online => online[0]).ToList();

                var result = UserHttpClient.Instance.GetCharacterListEvents(uploadList);

                if (result != null)
                {
                    result = result.Where(s => s.EventType == EventType.INSTANTBATTLE).OrderByDescending(s => s.LevelSum)?.ThenBy(s => s.CharacterId);
                    var selectedCharacter = result.FirstOrDefault();

                    if (selectedCharacter == null)
                    {
                        return;
                    }

                    if (session.Character.CharacterId != selectedCharacter.CharacterId)
                    {
                        session.SendPacket(session.Character.GenerateSay($"Character {selectedCharacter.Name} received rewards.", 11));
                        return;
                    }
                }
            }

            if (session.Character.EffectFromTitle == null)
            {
                session.Character.EffectFromTitle = new ThreadSafeGenericList<BCard>();
            }

            var hasEndlessBattleTitle = session.Character.EffectFromTitle.Any(s => s.Type == (byte)BCardType.CardType.BonusBCards && s.SubType == (byte)AdditionalTypes.BonusTitleBCards.InstantBattleAFKPower);

            
            var reputation = (int)(((session.Character.Level + session.Character.HeroLevel) * 50) * (hasEndlessBattleTitle ? 1.5 : 1));
            var gold = (int)(((session.Character.Level + session.Character.HeroLevel) * 1000 * ServerManager.Instance.Configuration.RateGold * 10) * (hasEndlessBattleTitle ? 1.5 : 1));

            bool alreadyDoneIc =
                session.Character.InstantBattleLogs.GetAllItems().OrderBy(s => s.DateTime)
                    .LastOrDefault()?.DateTime.Date == DateTime.Today;

            if (alreadyDoneIc)
            {
                session.SendPacket(session.Character.GenerateSay("You have already done an instant battle today, you will not receive daily rewards.", 11));
            }
            else
            {
                session.Character.GiftAdd(1403, 1);
                session.Character.InstantBattleLogs.Add(new InstantBattleLogDTO
                {
                    CharacterId = session.Character.CharacterId,
                    DateTime = DateTime.Now
                });
            }

            // Only for c30+ IB
            if (mapInstance.Item2 == 130)
            {
                session.Character.GiftAdd(1404, 1);
            }
            var family = (FamilyDTO)session.Character.Family;
            session.Character.GetReputation(reputation);
            session.Character.GetGold(gold);
            if (family != null && family.FamilyGold + 20000 * ServerManager.Instance.Configuration.RateFamGold <= ServerManager.Instance.Configuration.MaxFamilyBankGold)
            {
                session?.SendPacket(session?.Character.GenerateSay($"You earned {20000 * ServerManager.Instance.Configuration.RateFamGold} Gold for your familybank.", 12));
                family.FamilyGold += 20000 * ServerManager.Instance.Configuration.RateFamGold;
                DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
                ServerManager.Instance.FamilyRefresh(family.FamilyId);
            }
            session.Character.SpAdditionPoint += session.Character.Level * 100;

            if (session.Character.SpAdditionPoint > 1000000)
            {
                session.Character.SpAdditionPoint = 1000000;
            }

            session.SendPacket(session.Character.GenerateSpPoint());
            session.SendPacket(session.Character.GenerateGold());
            session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_SP_POINT"), session.Character.Level * 100), 10));
        }

        public void SendRaidRewards(ClientSession session, Group group)
        {
            short teamSize = group.Raid.InstanceBag.Lives;

            foreach (Gift gift in group.Raid.GiftItems)
            {
                if (group.Sessions.CountLinq(se => se.CleanIpAddress.Equals(session.CleanIpAddress)) > 2)
                {
                    session.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("MAX_PLAYER_ALLOWED"), 10));
                    continue;
                }

                byte rare = 0;

                for (int i = ItemHelper.RareRate.Length - 1, t = ServerManager.RandomNumber(); i >= 0; i--)
                {
                    if (t < ItemHelper.RareRate[i])
                    {
                        rare = (byte)i;
                        break;
                    }
                }

                if (rare < 1)
                {
                    rare = 1;
                }

                if (session.Character.Level >= group.Raid.LevelMinimum)
                { 
                    var increasePercentage = 0;
                    var rnd = ServerManager.RandomNumber();
                    increasePercentage += ServerManager.Instance.Configuration.BonusRaidBoxPercentage;

                    increasePercentage += session.Character.GetBuff(BCardType.CardType.BonusBCards, ((byte) AdditionalTypes.BonusTitleBCards.IncreaseRaidBoxDropChance))[0];

                    if ((gift.MinTeamSize == 0 && gift.MaxTeamSize == 0) || (teamSize >= gift.MinTeamSize && teamSize <= gift.MaxTeamSize))
                    {
                        if (increasePercentage != 0 && rnd <= increasePercentage)
                        {
                            session.Character.Session?.SendPacket(session.Character.GenerateSay(Language.Instance.GetMessageFromKey("RECEIVE_RAIDBOX"), 12));
                            session.Character.GiftAdd(gift.VNum, gift.Amount, rare, 0, gift.Design, gift.IsRandomRare);
                            session.CurrentMapInstance?.Broadcast(StaticPacketHelper.GenerateEff(UserType.Player, session.Character.CharacterId, 5004), session.Character.PositionX, session.Character.PositionY);

                        }
                        session.Character.GiftAdd(gift.VNum, gift.Amount, rare, 0, gift.Design, gift.IsRandomRare);
                    }
                }
                Thread.Sleep(100);
            }

            session.Character.GetReputation(group.Raid.Reputation);

            if (group.Raid.Id == 25)
            {
                Random rnd = new Random();
                var boxraid = rnd.Next(100);

                if (boxraid <= 50)
                {
                    session.Character.GiftAdd(4717, 1);
                    session.SendPacket(session.Character.GenerateSay("You received the Ancelloan the Creator Treasure Chest x1", 11));
                }
                else
                {
                    session.Character.GiftAdd(4718, 1);
                    session.SendPacket(session.Character.GenerateSay("You received the Fernon the Destroyer Treasure Chest x1", 11));
                }
            }

            if (session.Character.Level >= group.Raid.LevelMaximum)
            {
                session.Character.GiftAdd(2320, 1);
                session.SendPacket(session.Character.GenerateSay("You won a raid certificate, you can sell it to a npc for 1.000.000 gold !", 11));
            }

            var sameRaid = ServerManager.Instance.Raids.Where(s => s.Id == session.Character.Group.Raid.Id);
           
            var family = (FamilyDTO)session.Character.Family;

            if (family != null && group.Raid.FamGold > 0)
            {
                if (family.FamilyGold + group.Raid.FamGold * ServerManager.Instance.Configuration.RateFamGold <= ServerManager.Instance.Configuration.MaxFamilyBankGold)
                {
                    session?.SendPacket(session?.Character.GenerateSay($"You earned {group.Raid.FamGold * ServerManager.Instance.Configuration.RateFamGold} Gold for your familybank.", 12));
                    family.FamilyGold += group.Raid.FamGold * ServerManager.Instance.Configuration.RateFamGold;
                    DAOFactory.FamilyDAO.InsertOrUpdate(ref family);
                    ServerManager.Instance.FamilyRefresh(family.FamilyId);
                }
            }

            if (session.Character.GenerateFamilyXp(group.Raid.FamExp, group.Raid.Id))
            {
                session.SendPacket(session.Character.GenerateSay(string.Format(Language.Instance.GetMessageFromKey("WIN_FXP"), group.Raid.FamExp * ServerManager.Instance.Configuration.RateFxp), 10));
            }

            session.Character.IncrementQuests(QuestType.WinRaid, group.Raid.Id);
        }
    }
}
