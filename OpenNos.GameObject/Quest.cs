using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class Quest : QuestDTO
    {
        #region Instantiation

        public Quest(QuestDTO input)
        {
            QuestId = input.QuestId;
            QuestType = input.QuestType;
            LevelMin = input.LevelMin;
            LevelMax = input.LevelMax;
            StartDialogId = input.StartDialogId;
            EndDialogId = input.EndDialogId;
            DialogNpcVNum = input.DialogNpcVNum;
            DialogNpcId = input.DialogNpcId;
            TargetMap = input.TargetMap;
            TargetX = input.TargetX;
            TargetY = input.TargetY;
            InfoId = input.InfoId;
            NextQuestId = input.NextQuestId;
            IsDaily = input.IsDaily;
            HasRandomRewards = input.HasRandomRewards;
            LoadSqstPosition();
        }

        #endregion

        #region Properties

        public List<QuestObjectiveDTO> QuestObjectives { get; set; }

        public List<QuestRewardDTO> QuestRewards { get; set; }

        public int SqstPosition { get; set; }

        #endregion

        #region Methods

        public string GetRewardPacket(Character character, bool onlyItems = false)
        {
            if (!QuestRewards.Any() || onlyItems && !QuestRewards.Any(s => (QuestRewardType)s.RewardType == QuestRewardType.WearItem || (QuestRewardType)s.RewardType == QuestRewardType.EtcMainItem))
            {
                return "";
            }

            return $"qr {GetRewardPacket()} {InfoId}";

            string GetRewardPacket()
            {
                string str = "";

                if (HasRandomRewards)
                {
                    var rnd = ServerManager.RandomNumber(0, QuestRewards.Count);
                    var reward = QuestRewards.ElementAt(rnd);

                    if (reward == null)
                    {
                        return str;
                    }

                    switch ((QuestRewardType) reward.RewardType)
                    {
                        // Item
                        case QuestRewardType.WearItem:
                        case QuestRewardType.EtcMainItem:
                            byte amount = (byte)(reward.Amount == 0 ? 1 : reward.Amount);
                            if (reward.Data == 1917 && character.IsMorphed)
                            {
                                amount *= 2;
                            }
                            character.GiftAdd((short)reward.Data, amount, reward.Rarity, reward.Upgrade, reward.Design, false);
                            str += $"{reward.RewardType} {reward.Data} {amount} ";
                            break;
                    }
                }
                else
                {
                    for (int a = 0; a < 4; a++)
                    {
                        QuestRewardDTO reward = QuestRewards.Skip(a).FirstOrDefault();
                        if (reward == null || onlyItems && (QuestRewardType)reward.RewardType != QuestRewardType.WearItem && (QuestRewardType)reward.RewardType != QuestRewardType.EtcMainItem)
                        {
                            str += "0 0 0 ";
                            continue;
                        }
                        switch ((QuestRewardType)reward.RewardType)
                        {
                            // Item
                            case QuestRewardType.WearItem:
                            case QuestRewardType.EtcMainItem:
                                byte amount = (byte)(reward.Amount == 0 ? 1 : reward.Amount);
                                if (reward.Data == 1917 && character.IsMorphed)
                                {
                                    amount *= 2;
                                }
                                character.GiftAdd((short)reward.Data, amount, reward.Rarity, reward.Upgrade, reward.Design, false);
                                str += $"{reward.RewardType} {reward.Data} {amount} ";
                                break;

                            // Gold
                            case QuestRewardType.Gold:
                            case QuestRewardType.SecondGold:
                            case QuestRewardType.ThirdGold:
                            case QuestRewardType.FourthGold:
                                character.GetGold(reward.Amount, true);
                                str += $"{reward.RewardType} 0 {(reward.Amount == 0 ? 1 : reward.Amount)} ";
                                break;

                            case QuestRewardType.Reput: // Reputation
                                character.GetReputation(reward.Amount);
                                str += $"{reward.RewardType} 0 0";
                                break;

                            case QuestRewardType.Exp: // Experience
                                if (character.Level < ServerManager.Instance.Configuration.MaxLevel)
                                {
                                    character.GetXp((long)(CharacterHelper.XPData[reward.Data > 255 ? 255 : reward.Data - 1] * reward.Amount / 100D));
                                }
                                str += $"{reward.RewardType} 0 0 ";
                                break;

                            case QuestRewardType.HExp: // % HeroExperience
                                if (character.Level < ServerManager.Instance.Configuration.MaxLevel)
                                {
                                    character.GetHeroXp((long)(CharacterHelper.HeroXpData[reward.Data > 59 ? 59 : reward.Data] * (reward.Amount / 100D)));

                                }

                                str += $"{reward.RewardType} 0 0 ";
                                break;

                            case QuestRewardType.SecondExp: // % Experience
                                if (character.Level < ServerManager.Instance.Configuration.MaxLevel)
                                {
                                    character.GetXp((long)(CharacterHelper.XPData[character.Level - 1] * reward.Amount / 100D));
                                }
                                str += $"{reward.RewardType} 0 0 ";
                                break;

                            case QuestRewardType.JobExp: // JobExperience
                                character.GetJobExp((long)(CharacterHelper.JobXpData[(int)character.Class, reward.Data > 98 ? 98 : reward.Data] * reward.Amount / 100D));
                                str += $"{reward.RewardType} 0 0 ";
                                break;

                            case QuestRewardType.SecondJobExp: // % JobExperience
                                character.GetJobExp((long)(CharacterHelper.JobXpData[(int)character.Class, character.JobLevel] * reward.Amount / 100D));
                                str += $"{reward.RewardType} 0 0 ";
                                break;

                            default:
                                str += "0 0 0 ";
                                break;
                        }
                    }
                }

                
                return str;
            }
        }

        public string RemoveTargetPacket() => $"targetoff {TargetX} {TargetY} {TargetMap} {QuestId}";

        public string TargetPacket() => $"target {TargetX} {TargetY} {TargetMap} {QuestId}";

        private void LoadSqstPosition()
        {
            SqstPosition = (int)Math.Round(84 + (((double)QuestId - 5000) / 8) + (((double)QuestId - 5000) / 100), 0) + (QuestId - 5000 >= 59 ? 1 : 0) + (QuestId - 5000 >= 100 ? 1 : 0) + ((double)QuestId % 8 > 4 && QuestId - 5000 < 60 ? 1 : 0);
        }

        #endregion

        //public string ClearAllTarget() => $"targetoff 0 0 0 0";
    }
}