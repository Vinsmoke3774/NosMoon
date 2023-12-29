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
    public class LevelUpRewardsDao : ILevelUpRewardDao
    {
        private readonly  IMapper<LevelUpRewardDTO, LevelUpRewardEntity> _mapper = new LevelUpRewardMapper();

        public IEnumerable<LevelUpRewardDTO> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var entities = context.LevelUpRewards.ToList();

            return _mapper.Map(entities);
        }
    }
}
