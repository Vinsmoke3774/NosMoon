using OpenNos.Core.Logger;
using OpenNos.DAL.EF;
using OpenNos.DAL.EF.Helpers;
using OpenNos.DAL.Interface;
using OpenNos.Data;
using OpenNos.Data.Enums;
using OpenNos.Mapper;
using OpenNos.Mapper.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class ChatLogDao : IChatLogDao
    {
        private readonly IMapper<ChatLogDTO, ChatLogEntity> _mapper = new ChatLogMapper();

        private void Insert(ref List<ChatLogEntity> chatLogs, ChatLogDTO dto)
        {
            var entity = _mapper.Map(dto);

            if (entity == null)
            {
                return;
            }

            chatLogs.Add(entity);
        }

        private void Update(ref List<ChatLogEntity> chatLogs, ChatLogDTO dto, ChatLogEntity entity)
        {
            entity = _mapper.Map(dto);

            if (entity == null)
            {
                return;
            }

            chatLogs.Add(entity);
        }

        public SaveResult InsertOrUpdate(IEnumerable<ChatLogDTO> dtos)
        {
            var listInsert = new List<ChatLogEntity>();
            var listUpdate = new List<ChatLogEntity>();
            try
            {
                using var context = DataAccessHelper.CreateContext();

                foreach (var dto in dtos)
                {
                    var foundEntity = context.ChatLogs.FirstOrDefault(s => s.Id == dto.Id);

                    if (foundEntity == null)
                    {
                        Insert(ref listInsert, dto);
                    }
                    else
                    {
                        Update(ref listUpdate, dto, foundEntity);
                    }
                }

                context.ChatLogs.BulkInsert(listInsert);
                context.ChatLogs.BulkUpdate(listUpdate);
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }
    }
}
