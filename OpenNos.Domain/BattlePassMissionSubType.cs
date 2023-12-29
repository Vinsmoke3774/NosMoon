namespace OpenNos.Domain
{
    public enum BattlePassMissionSubType : byte
    {
        // [POSSIBLE DAILY]
        OpenMotherCubyRaidBoxes = 0,
        OpenGinsengRaidBoxes = 1,
        OpenGiantBlackSpiderRaidBoxes = 2,
        OpenDarkCastraRaidBoxes = 3,
        OpenMassiveSladeRaidBoxes = 4,
        OpenChickenKingRaidBoxes = 5,
        OpenNamajuRaidBoxes = 6,
        OpenFafnirRaidBoxes = 7,
        OpenYertirandRaidBoxes = 8,
        OpenKertosRaidBoxes = 9,
        OpenValakusRaidBoxes = 10,
        OpenGrenigasRaidBoxes = 11,
        CompleteLaurenaRaid = 12,
        CompleteLordDracoRaid = 13,
        CompleteGlacerusRaid = 14,

        // [POSSIBLE WEEKLY]
        WinRBBGames = 15,
        WinAoTGames = 16,
        KillPlayersInGlaceron = 17,
        WinRoundInAoT = 18,
        WinAct4Raid = 19,
        KillMonstersInPlayerRangeLevel = 20,

        // [POSSIBLE SEASONAL]
        WinStreakRBB = 21,
        ParticipateAoTRanked = 22,
        ParticipateRBB = 23,
        WinInstantCombat = 24,
        WinRaid = 25,
        KillFlyingFireDevil = 26,
        EarnFamilyActionPoints = 27,
    }
}