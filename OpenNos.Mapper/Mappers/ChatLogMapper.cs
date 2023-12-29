using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class ChatLogMapper : IMapper<ChatLogDTO, ChatLogEntity>
    {
        public ChatLogEntity Map(ChatLogDTO input)
        {
            if (input == null)
            {
                return null;
            }

            var result = new ChatLogEntity
            {
                CharacterId = input.CharacterId,
                CharacterName = input.CharacterName,
                MessageType = input.MessageType,
                Message = input.Message,
                DestinationCharacterId = input.DestinationCharacterId,
                DestinationCharacterName = input.DestinationCharacterName,
                DateTime = input.DateTime,
                FamilyId = input.FamilyId,
                FamilyName = input.FamilyName,
                NoteTitle = input.NoteTitle
            };

            if (result.Id == Guid.Empty || result.Id == new Guid())
            {
                result.Id = Guid.NewGuid();
            }

            return result;
        }

        public ChatLogDTO Map(ChatLogEntity input)
        {
            if (input == null)
            {
                return null;
            }

            var result = new ChatLogDTO
            {
                CharacterId = input.CharacterId,
                CharacterName = input.CharacterName,
                MessageType = input.MessageType,
                Message = input.Message,
                DestinationCharacterId = input.DestinationCharacterId,
                DestinationCharacterName = input.DestinationCharacterName,
                DateTime = input.DateTime,
                FamilyId = input.FamilyId,
                FamilyName = input.FamilyName,
                NoteTitle = input.NoteTitle
            };

            if (result.Id == Guid.Empty || result.Id == new Guid())
            {
                result.Id = Guid.NewGuid();
            }

            return result;
        }

        public IEnumerable<ChatLogDTO> Map(IEnumerable<ChatLogEntity> input)
        {
            var result = new List<ChatLogDTO>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }

        public IEnumerable<ChatLogEntity> Map(IEnumerable<ChatLogDTO> input)
        {
            var result = new List<ChatLogEntity>();

            foreach (var data in input)
            {
                result.Add(Map(data));
            }

            return result;
        }
    }
}
