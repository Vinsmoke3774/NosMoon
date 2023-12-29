namespace OpenNos.Domain
{
    public enum GuriType : int
    {
        DanceAnimation = 2,
        TextInput = 4,
        DanceAnimationPercent = 5,
        Emoticon = 10,
        FriendshipWings = 199,
        OpenPetBasket = 201,
        OpenPartnerBackPack = 202,
        SpecialistInitializationPotion = 203,
        IdentifyShell = 204,
        PerfumeItem = 205,
        AddMountInPearl = 208,
        AddFairyInPearl = 209,
        UseBoxItem = 300,
        UseBoxItem2 = 305,
        AddTitle = 306,
        CollectItem = 400,
        StartBattleRoyale = 501,
        UnfreezePlayer = 502,
        StartRbbCountdown = 503,
        TeleportToRbbBase = 504,
        EventWaiting = 506,
        RemoveLaurenaMorph = 513,
        Wedding = 603,
        SearchHiddenTimeSpace = 700,
        MapTeleporter = 710,
        TeleportOnOtherMapTp = 711, // Need to check, this isn't used in database for some reason
        AddFlagToRbbTeam = 720,
        FactionEgg = 750,
        ActVRelic = 1502,
        KingOfTheHillMatchmaking = 3000
    }

}
