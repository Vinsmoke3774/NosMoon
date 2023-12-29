namespace OpenNos.Domain
{
    public enum GroupAttackType : byte
    {
        Default,
        WhenMobAttackYou = 1,
        WhenYouAttackMob = 5,
        Pikanya = 6,
        Siren = 7,
        Merling = 8
    }
}
