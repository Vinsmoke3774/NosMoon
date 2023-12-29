using OpenNos.Data;
using OpenNos.Data.Enums;
using System.Collections.Generic;

namespace OpenNos.DAL.Interface
{
    public interface IQuestDAO
    {
        #region Methods

        DeleteResult DeleteById(long id);

        void Insert(List<QuestDTO> questList);

        QuestDTO InsertOrUpdate(QuestDTO quest);

        IEnumerable<QuestDTO> LoadAll();

        QuestDTO LoadById(long id);

        #endregion
    }
}