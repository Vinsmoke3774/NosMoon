using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class MimicRotationDAO : IMimicRotationDAO
    {
        private readonly IMapper<MimicRotationDTO, MimicRotation> _mapper = new MimicRotationMapper();

        public IEnumerable<MimicRotationDTO> LoadAll()
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var data = context.MimicRotation.ToList();
                var result = new List<MimicRotationDTO>();
                foreach (var item in data)
                {
                    result.Add(_mapper.Map(item));
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }

        public IEnumerable<MimicRotationDTO> LoadByRotationType(MimicRotationType type)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var data = context.MimicRotation.Where(s => s.RotationType == type);
                var result = new List<MimicRotationDTO>();
                foreach (var item in data)
                {
                    result.Add(_mapper.Map(item));
                }

                return result;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return null;
            }
        }
    }
}
