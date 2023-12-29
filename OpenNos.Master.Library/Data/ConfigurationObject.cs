using System;
using OpenNos.Domain;

namespace OpenNos.Master.Library.Data
{
    [Serializable]
    public class ConfigurationObject
    {
        #region Properties

        public short RateFxp { get; set; }

        public short RateFamGold { get; set; }

        public string Act4IP { get; set; }

        public int Act4Port { get; set; }

        public bool ChristmasEvent { get; set; }

        public byte CylloanPercentRate { get; set; }

        public bool HalloweenEvent { get; set; }

        public byte HeroicStartLevel { get; set; }

        public string MallAPIKey { get; set; }

        public string MallBaseURL { get; set; }

        public long MaxGold { get; set; }

        public long MaxGoldBank { get; set; }

        public long MaxFamilyBankGold { get; set; }

        public byte MaxHeroLevel { get; set; }

        public byte MaxJobLevel { get; set; }

        public byte MaxLevel { get; set; }

        public byte MaxSPLevel { get; set; }

        public byte MaxUpgrade { get; set; }

        public long PartnerSpXp { get; set; }

        public int QuestDropRate { get; set; }

        public int RateDrop { get; set; }

        public int RateFairyXP { get; set; }

        public int RateGold { get; set; }

        public int RateGoldDrop { get; set; }

        public int RateHeroicXP { get; set; }

        public int RateReputation { get; set; }

        public int RateXP { get; set; }

        public int RateJXP { get; set; }

        public int RateAct4Xp { get; set; }

        public bool SceneOnCreate { get; set; }

        public int SessionLimit { get; set; }

        public bool UseLogService { get; set; }

        public bool WorldInformation { get; set; }

        public MimicRotationType MimicRotationType { get; set; }

        public string GoogleIssuer { get; set; }

        public string GoogleTitleNoSpace { get; set; }

        public string GoogleAuthKey { get; set; }

        public int BonusRaidBoxPercentage { get; set; }

        public short ReputationDifficultyMultiplier { get; set; }

        public bool RaidPortalLimitation { get; set; }

        public DateTime EndSeason { get; set; }

        public bool BattlePassIconEnabled { get; set; }

        public short MaxBattlePassPoints { get; set;}

        public int RateSpXP { get; set; }

        #endregion
    }
}