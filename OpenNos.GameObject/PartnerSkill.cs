﻿using OpenNos.Data;
using OpenNos.GameObject.Networking;
using System;

namespace OpenNos.GameObject
{
    public class PartnerSkill
    {
        #region Members

        private Skill _skill;

        #endregion

        #region Instantiation

        public PartnerSkill(PartnerSkillDTO input)
        {
            PartnerSkillId = input.PartnerSkillId;
            EquipmentSerialId = input.EquipmentSerialId;
            SkillVNum = input.SkillVNum;
            Level = input.Level;
        }

        #endregion

        #region Properties

        public Guid EquipmentSerialId { get; set; }

        public DateTime LastUse { get; set; }

        public byte Level { get; set; }

        public long PartnerSkillId { get; set; }

        public Skill Skill => _skill ?? (_skill = ServerManager.GetSkill(SkillVNum));

        public short SkillVNum { get; set; }

        #endregion

        #region Methods

        public bool CanBeUsed() => Skill != null && LastUse.AddMilliseconds(Skill.Cooldown * 100) < DateTime.Now;

        #endregion
    }
}