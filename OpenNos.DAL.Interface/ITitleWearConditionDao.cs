using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;

namespace OpenNos.DAL.Interface
{
    public interface ITitleWearConditionDao
    {
        IEnumerable<TitleWearConditionDTO> LoadAll();
    }
}
