using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public static class NpcMonsterMapper
    {
        #region Methods

        public static bool ToNpcMonster(NpcMonsterDTO input, NpcMonster output)
        {
            if (input == null) return false;
            output.AmountRequired = input.AmountRequired;
            output.AttackClass = input.AttackClass;
            output.AttackUpgrade = input.AttackUpgrade;
            output.BasicArea = input.BasicArea;
            output.BasicCooldown = input.BasicCooldown;
            output.BasicRange = input.BasicRange;
            output.BasicSkill = input.BasicSkill;
            output.Catch = input.Catch;
            output.CloseDefence = input.CloseDefence;
            output.Concentrate = input.Concentrate;
            output.CriticalChance = input.CriticalChance;
            output.CriticalRate = input.CriticalRate;
            output.DamageMaximum = input.DamageMaximum;
            output.DamageMinimum = input.DamageMinimum;
            output.DarkResistance = input.DarkResistance;
            output.DefenceDodge = input.DefenceDodge;
            output.DefenceUpgrade = input.DefenceUpgrade;
            output.DistanceDefence = input.DistanceDefence;
            output.DistanceDefenceDodge = input.DistanceDefenceDodge;
            output.Element = input.Element;
            output.ElementRate = input.ElementRate;
            output.FireResistance = input.FireResistance;
            output.HeroLevel = input.HeroLevel;
            output.IsHostile = input.IsHostile;
            output.JobXP = input.JobXP;
            output.Level = input.Level;
            output.LightResistance = input.LightResistance;
            output.MagicDefence = input.MagicDefence;
            output.MaxHP = input.MaxHP;
            output.MaxMP = input.MaxMP;
            output.MonsterType = input.MonsterType;
            output.Name = input.Name;
            output.NoAggresiveIcon = input.NoAggresiveIcon;
            output.NoticeRange = input.NoticeRange;
            output.NpcMonsterVNum = input.NpcMonsterVNum;
            output.OriginalNpcMonsterVNum = input.OriginalNpcMonsterVNum;
            output.Race = input.Race;
            output.RaceType = input.RaceType;
            output.RespawnTime = input.RespawnTime;
            output.Speed = input.Speed;
            output.VNumRequired = input.VNumRequired;
            output.WaterResistance = input.WaterResistance;
            output.XP = input.XP;
            output.EvolutionVNum = input.EvolutionVNum;
            output.EvolutionChance = input.EvolutionChance;
            output.BuffId = input.BuffId;
            output.IsMovable = input.IsMovable;
            output.IsPercent = input.IsPercent;
            output.TakeDamages = input.TakeDamages;
            output.GiveDamagePercentage = input.GiveDamagePercentage;
            output.GroupAttack = input.GroupAttack;
            output.HpData = input.HpData;
            output.MpData = input.MpData;
            output.PetInfo1 = input.PetInfo1;
            output.PetInfo2 = input.PetInfo2;
            output.PetInfo3 = input.PetInfo3;
            output.PetInfo4 = input.PetInfo4;
            output.Winfo1 = input.Winfo1;
            output.Winfo2 = input.Winfo2;
            output.Winfo3 = input.Winfo3;
            output.WeaponData1 = input.WeaponData1;
            output.WeaponData2 = input.WeaponData2;
            output.WeaponData3 = input.WeaponData3;
            output.WeaponData4 = input.WeaponData4;
            output.WeaponData5 = input.WeaponData5;
            output.WeaponData6 = input.WeaponData6;
            output.WeaponData7 = input.WeaponData7;
            output.Ainfo1 = input.Ainfo1;
            output.Ainfo2 = input.Ainfo2;
            output.ArmorData1 = input.ArmorData1;
            output.ArmorData2 = input.ArmorData2;
            output.ArmorData3 = input.ArmorData3;
            output.ArmorData4 = input.ArmorData4;
            output.ArmorData5 = input.ArmorData5;

            return true;
        }

        public static bool ToNpcMonsterDTO(NpcMonster input, NpcMonsterDTO output)
        {
            if (input == null) return false;

            output.AmountRequired = input.AmountRequired;
            output.AttackClass = input.AttackClass;
            output.AttackUpgrade = input.AttackUpgrade;
            output.BasicArea = input.BasicArea;
            output.BasicCooldown = input.BasicCooldown;
            output.BasicRange = input.BasicRange;
            output.BasicSkill = input.BasicSkill;
            output.Catch = input.Catch;
            output.CloseDefence = input.CloseDefence;
            output.Concentrate = input.Concentrate;
            output.CriticalChance = input.CriticalChance;
            output.CriticalRate = input.CriticalRate;
            output.DamageMaximum = input.DamageMaximum;
            output.DamageMinimum = input.DamageMinimum;
            output.DarkResistance = (sbyte)input.DarkResistance;
            output.DefenceDodge = input.DefenceDodge;
            output.DefenceUpgrade = input.DefenceUpgrade;
            output.DistanceDefence = input.DistanceDefence;
            output.DistanceDefenceDodge = input.DistanceDefenceDodge;
            output.Element = input.Element;
            output.ElementRate = input.ElementRate;
            output.FireResistance = (sbyte)input.FireResistance;
            output.HeroLevel = input.HeroLevel;
            output.IsHostile = input.IsHostile;
            output.JobXP = input.JobXP;
            output.Level = input.Level;
            output.LightResistance = (sbyte)input.LightResistance;
            output.MagicDefence = input.MagicDefence;
            output.MaxHP = input.MaxHP;
            output.MaxMP = input.MaxMP;
            output.MonsterType = input.MonsterType;
            output.Name = input.Name;
            output.NoAggresiveIcon = input.NoAggresiveIcon;
            output.NoticeRange = input.NoticeRange;
            output.NpcMonsterVNum = input.NpcMonsterVNum;
            output.OriginalNpcMonsterVNum = input.OriginalNpcMonsterVNum;
            output.Race = input.Race;
            output.RaceType = input.RaceType;
            output.RespawnTime = input.RespawnTime;
            output.Speed = input.Speed;
            output.VNumRequired = input.VNumRequired;
            output.WaterResistance = (sbyte)input.WaterResistance;
            output.XP = input.XP;
            output.EvolutionVNum = input.EvolutionVNum;
            output.EvolutionChance = input.EvolutionChance;
            output.BuffId = input.BuffId;
            output.IsMovable = input.IsMovable;
            output.IsPercent = input.IsPercent;
            output.TakeDamages = input.TakeDamages;
            output.GiveDamagePercentage = input.GiveDamagePercentage;
            output.GroupAttack = input.GroupAttack;
            output.HpData = input.HpData;
            output.MpData = input.MpData;
            output.PetInfo1 = input.PetInfo1;
            output.PetInfo2 = input.PetInfo2;
            output.PetInfo3 = input.PetInfo3;
            output.PetInfo4 = input.PetInfo4;
            output.Winfo1 = input.Winfo1;
            output.Winfo2 = input.Winfo2;
            output.Winfo3 = input.Winfo3;
            output.WeaponData1 = input.WeaponData1;
            output.WeaponData2 = input.WeaponData2;
            output.WeaponData3 = input.WeaponData3;
            output.WeaponData4 = input.WeaponData4;
            output.WeaponData5 = input.WeaponData5;
            output.WeaponData6 = input.WeaponData6;
            output.WeaponData7 = input.WeaponData7;
            output.Ainfo1 = input.Ainfo1;
            output.Ainfo2 = input.Ainfo2;
            output.ArmorData1 = input.ArmorData1;
            output.ArmorData2 = input.ArmorData2;
            output.ArmorData3 = input.ArmorData3;
            output.ArmorData4 = input.ArmorData4;
            output.ArmorData5 = input.ArmorData5;

            return true;
        }

        #endregion
    }
}