using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IPartnerSkillDAO
    {
        #region Methods

        PartnerSkillDTO Insert(PartnerSkillDTO partnerSkillDTO);

        List<PartnerSkillDTO> LoadByEquipmentSerialId(Guid equipmentSerialId);

        DeleteResult Remove(long partnerSkillId);

        #endregion
    }
}