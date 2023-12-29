using System.Collections.Generic;
using OpenNos.DAL.EF;
using OpenNos.Data;

namespace OpenNos.Mapper.Mappers
{
    //public class CellonOptionMapper : IMapper<CellonOptionDTO, CellonOption>
    //{
    //    public CellonOption Map(CellonOptionDTO input)
    //    {
    //        if (input == null)
    //        {
    //            return null;
    //        }

    //        return new CellonOption
    //        {
    //            CellonOptionId = input.CellonOptionId,
    //            EquipmentSerialId = input.EquipmentSerialId,
    //            Level = input.Level,
    //            Type = input.Type,
    //            Value = input.Value
    //        };
    //    }

    //    public CellonOptionDTO Map(CellonOption input)
    //    {
    //        if (input == null)
    //        {
    //            return null;
    //        }

    //        return new CellonOptionDTO
    //        {
    //            CellonOptionId = input.CellonOptionId,
    //            EquipmentSerialId = input.EquipmentSerialId,
    //            Level = input.Level,
    //            Type = input.Type,
    //            Value = input.Value
    //        };
    //    }

    //    public IEnumerable<CellonOptionDTO> Map(IEnumerable<CellonOption> input)
    //    {
    //        var result = new List<CellonOptionDTO>();

    //        foreach (var data in input)
    //        {
    //            result.Add(Map(data));
    //        }

    //        return result;
    //    }

    //    public IEnumerable<CellonOption> Map(IEnumerable<CellonOptionDTO> input)
    //    {
    //        var result = new List<CellonOption>();

    //        foreach (var data in input)
    //        {
    //            result.Add(Map(data));
    //        }

    //        return result;
    //    }
    //}
    public static class CellonOptionMapper
    {
        #region Methods

        public static bool ToCellonOption(CellonOptionDTO input, CellonOption output)
        {
            if (input == null)
            {
                return false;
            }

            output.CellonOptionId = input.CellonOptionId;
            output.EquipmentSerialId = input.EquipmentSerialId;
            output.Level = input.Level;
            output.Type = input.Type;
            output.Value = input.Value;

            return true;
        }

        public static bool ToCellonOptionDTO(CellonOption input, CellonOptionDTO output)
        {
            if (input == null)
            {
                return false;
            }

            output.CellonOptionId = input.CellonOptionId;
            output.EquipmentSerialId = input.EquipmentSerialId;
            output.Level = input.Level;
            output.Type = input.Type;
            output.Value = input.Value;

            return true;
        }

        #endregion
    }

}