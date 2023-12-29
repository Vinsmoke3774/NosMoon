using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;

namespace OpenNos.DAL.DAO
{
    public class TitleWearConditionDao : ITitleWearConditionDao
    {
        private readonly IMapper<TitleWearConditionDTO, TitleWearConditionEntity> _mapper = new TitleWearConditionMapper();

        public IEnumerable<TitleWearConditionDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.TitleWearCondition.ToList();

            return _mapper.Map(entities);
        }
    }
}
