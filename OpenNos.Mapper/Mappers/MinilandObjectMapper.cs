using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    public class CharacterTimespaceLogMapper
    {
        public static bool ToCharacterTimespaceLogEntity(CharacterTimespaceLogDTO input, CharacterTimespaceLog output)
        {
            if (input == null)
            {
                return false;
            }

            output.LogId = input.CharacterId;
            output.CharacterId = input.CharacterId;
            output.ScriptedInstanceId = input.ScriptedInstanceId;
            output.Score = input.Score;
            output.Date = input.Date;
            output.IsFailed = input.IsFailed;

            return true;
        }

        public static bool ToCharacterTimespaceLogDto(CharacterTimespaceLog input, CharacterTimespaceLogDTO output)
        {
            if (input == null)
            {
                return false;
            }

            output.LogId = input.CharacterId;
            output.CharacterId = input.CharacterId;
            output.ScriptedInstanceId = input.ScriptedInstanceId;
            output.Score = input.Score;
            output.Date = input.Date;
            output.IsFailed = input.IsFailed;

            return true;
        }
    }
}
