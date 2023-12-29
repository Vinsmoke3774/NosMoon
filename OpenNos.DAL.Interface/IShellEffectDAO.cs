using OpenNos.Data;
using OpenNos.Data.Enums;
using System;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IShellEffectDAO// : IGenericMongoAccessor<ShellEffectDTO>
    {
        #region Methods

        DeleteResult DeleteByEquipmentSerialId(Guid id, bool isRune = false);

        ShellEffectDTO InsertOrUpdate(ShellEffectDTO shelleffect);

        DeleteResult DeleteOption(Guid itemId, ShellEffectDTO option, bool isRune = false);

        void InsertOrUpdateFromList(List<ShellEffectDTO> shellEffects, Guid equipmentSerialId);

        IEnumerable<ShellEffectDTO> LoadByEquipmentSerialId(Guid id, bool isRune = false);

        void InsertOrUpdateFromList(IDictionary<Guid, IList<ShellEffectDTO>> effects);

        void InsertFromList(List<ShellEffectDTO> shellEffects, Guid equipmentSerialId);

        List<ShellEffectDTO> LoadAll();

        #endregion
    }
}