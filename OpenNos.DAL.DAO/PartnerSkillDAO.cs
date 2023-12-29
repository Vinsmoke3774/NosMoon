using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class PartnerSkillDAO : IPartnerSkillDAO
    {
        #region Methods

        public PartnerSkillDTO Insert(PartnerSkillDTO partnerSkillDTO)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var partnerSkill = new PartnerSkill();

                if (Mapper.Mappers.PartnerSkillMapper.ToPartnerSkill(partnerSkillDTO, partnerSkill))
                {
                    context.PartnerSkill.Add(partnerSkill);
                    context.SaveChanges();

                    var dto = new PartnerSkillDTO();

                    if (Mapper.Mappers.PartnerSkillMapper.ToPartnerSkillDTO(partnerSkill, dto))
                    {
                        return dto;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }

            return null;
        }

        public List<PartnerSkillDTO> LoadByEquipmentSerialId(Guid equipmentSerialId)
        {
            var partnerSkillDTOs = new List<PartnerSkillDTO>();

            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.PartnerSkill.Where(s => s.EquipmentSerialId == equipmentSerialId).ToList()
                    .ForEach(partnerSkill =>
                    {
                        var partnerSkillDTO = new PartnerSkillDTO();

                        if (Mapper.Mappers.PartnerSkillMapper.ToPartnerSkillDTO(partnerSkill, partnerSkillDTO))
                        {
                            partnerSkillDTOs.Add(partnerSkillDTO);
                        }
                    });
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }

            return partnerSkillDTOs;
        }

        public DeleteResult Remove(long partnerSkillId)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var partnerSkill = context.PartnerSkill.FirstOrDefault(s => s.PartnerSkillId == partnerSkillId);

                if (partnerSkill == null)
                {
                    return DeleteResult.NotFound;
                }

                context.PartnerSkill.Remove(partnerSkill);
                context.SaveChanges();

                return DeleteResult.Deleted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }

            return DeleteResult.Error;
        }

        #endregion
    }
}