using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.GameObject.Helpers;
using OpenNos.GameObject.Networking;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.GameObject
{
    public class PartnerSp
    {
        #region Instantiation

        public PartnerSp(ItemInstance instance)
        {
            Instance = instance;

            Initialize();
        }

        #endregion

        #region Properties

        public ItemInstance Instance { get; }

        private List<PartnerSkill> Skills { get; set; }

        private long XpMax => ServerManager.Instance.Configuration.PartnerSpXp;

        #endregion

        #region Methods

        public bool AddSkill(Mate mate, byte castId)
        {
            Skill skillSp = new Skill();
            foreach (Skill ski in ServerManager.GetAllSkill())
            {
                if (ski.UpgradeType == mate.Sp.Instance.Item.MorphSp && ski.SkillType == 5 && ski.CastId == castId)
                {
                    skillSp = ski;
                }
            }

            if (skillSp != null)
            {
                PartnerSkillDTO partnerSkillDTO = new PartnerSkillDTO
                {
                    EquipmentSerialId = Instance.EquipmentSerialId,
                    SkillVNum = skillSp.SkillVNum,
                    Level = ServerManager.RandomNumber<byte>(1, 8)
                };

                if (DAOFactory.PartnerSkillDAO.Insert(partnerSkillDTO) is PartnerSkillDTO result)
                {
                    Skills.Add(new PartnerSkill(result));

                    return true;
                }
            }
            else
            {
                Logger.Log.Warn($"Partner skill not found (Morph: {Instance.Item.Morph}, CastId: {castId})");
            }

            return false;
        }

        public void AddXp(long amount)
        {
            if (Instance.XP < XpMax)
            {
                Instance.XP = Math.Min(Instance.XP + amount, XpMax);
            }
        }

        public bool CanLearnSkill() => Instance.XP >= XpMax;

        public void ClearSkills()
        {
            for (byte castId = 0; castId < 3; castId++)
            {
                RemoveSkill(castId);
            }

            ReloadSkills();
        }

        public void FullXp()
        {
            Instance.XP = XpMax;
        }

        public string GeneratePski()
        {
            string pski = "pski";

            foreach (PartnerSkill partnerSkill in Skills.OrderBy(s => s.Skill.CastId))
            {
                pski += $" {partnerSkill.Skill.SkillVNum}";
            }

            return pski;
        }

        public PartnerSkillBullshit GenerateSkills(bool withLevel = true)
        {
            var bullshit = new PartnerSkillBullshit();
            string skills = "";

            for (byte castId = 0; castId < 3; castId++)
            {
                PartnerSkill partnerSkill = GetSkill(castId);

                if (partnerSkill != null)
                {
                    if (partnerSkill.Level == 7)
                    {
                        switch (castId)
                        {
                            case 0:
                                bullshit.Effect1 = 4237;
                                break;
                            case 1:
                                bullshit.Effect2 = 4238;
                                break;
                            case 2:
                                bullshit.Effect3 = 4239;
                                break;
                        }
                    }
                    skills += withLevel ? $" {partnerSkill.SkillVNum}.{partnerSkill.Level}" : $" {partnerSkill.SkillVNum}";
                }
                else
                {
                    skills += withLevel ? $" 0.0" : $" 0";
                }
            }

            bullshit.ApparentlySkillVnumsBecauseSaluCodesLikeCrapShit = skills;

            return bullshit;
        }

        public int GetCooldown()
        {
            double maxCooldown = 30000;

            foreach (PartnerSkill partnerSkill in Skills.ToList().Where(s => !s.CanBeUsed()))
            {
                double remaining = (partnerSkill.LastUse - DateTime.Now).TotalMilliseconds + (partnerSkill.Skill.Cooldown * 100);

                if (remaining > maxCooldown)
                {
                    maxCooldown = remaining;
                }
            }

            return (int)(maxCooldown / 1000D);
        }

        public int GetLevelForAllSkill()
        {
            var value = 0;
            foreach (var skill in Skills)
            {
                value += skill.Level;
            }
            return value;
        }

        public string GetName()
        {
            switch (Instance.ItemVNum)
            {
                case 4324: return "Guardian^Lucifer";
                case 4325: return "Sheriff^Chloe";
                case 4326: return "Bone^Warrior^Ragnar";
                case 4343: return "Mad^Professor^Macavity";
                case 4349: return "Archdaemon^Amon";
                case 4405: return "Magic^Student^Yuna";
                case 4413: return "Amora";
                case 4417: return "Mad^March^Hare";
                case 4800: return "Aegir^the^Angry";
                case 4802: return "Barni^the^Clever";
                case 4803: return "Freya^the^Fateful";
                case 4804: return "Shinobi^the^Silent";
                case 4805: return "Lotus^the^Graceful";
                case 4806: return "Orkani^the^Turbulent";
                case 4807: return "Foxy";
                case 4808: return "Maru";
                case 4809: return "Maru^in^Mother's^Fur";
                case 4810: return "Hongbi";
                case 4811: return "Cheongbi";
                case 4812: return "Lucifer";
                case 4813: return "Witch^Laurena";
                case 4814: return "Amon";
                case 4815: return "Lucy^Lopea﻿rs";
                case 4817: return "Cowgirl^Chloe";
                case 4818: return "Fiona";
                case 4819: return "Jinn";
                case 4820: return "Ice^Princess^Eliza";
                case 4821: return "Daniel^Ducats";
                case 4822: return "Palina^Puppet^Master";
                case 4823: return "Harlequin";
                case 4824: return "Nelia^Nymph";
                case 4825: return "Little^Pri﻿ncess^Venus";
            }

            return Instance.Item.Name.Replace(' ', '^');
        }

        public PartnerSkill GetSkill(byte castId)
            => Skills.FirstOrDefault(s => s.Skill.CastId == castId);

        public int GetSkillsCount() => Skills.Count;

        public int GetXpPercent() => (int)Math.Floor(100D / XpMax * Instance.XP);

        public void ReloadSkills() => LoadSkills();

        public bool RemoveSkill(byte castId)
        {
            PartnerSkill partnerSkill = GetSkill(castId);

            return partnerSkill != null && DAOFactory.PartnerSkillDAO.Remove(partnerSkill.PartnerSkillId) != DeleteResult.Error;
        }

        public void ResetXp()
        {
            Instance.XP = 0;
        }

        private Skill GetNewSkill(byte castId) => ServerManager.GetAllSkill().FirstOrDefault(s => s.CastId == castId && (s.Class == 28 || s.Class == 29)
                                                            && s.UpgradeType == MateHelper.Instance.GetUpgradeType(Instance.Item.Morph));

        private void Initialize()
        {
            if (Instance.EquipmentSerialId == Guid.Empty)
            {
                Instance.EquipmentSerialId = Guid.NewGuid();
            }

            LoadSkills();
        }

        private void LoadSkills()
        {
            Skills = DAOFactory.PartnerSkillDAO.LoadByEquipmentSerialId(Instance.EquipmentSerialId)
                .Select(partnerSkillDTO => new PartnerSkill(partnerSkillDTO)).ToList();
        }

        #endregion
    }

    public class PartnerSkillBullshit
    {
        public string ApparentlySkillVnumsBecauseSaluCodesLikeCrapShit { get; set; }

        public short Effect1 { get; set; }

        public short Effect2 { get; set; }

        public short Effect3 { get; set; }
    }
}