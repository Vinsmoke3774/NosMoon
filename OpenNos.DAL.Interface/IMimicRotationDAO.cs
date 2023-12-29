using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.DAL.Interface
{
    public interface IMimicRotationDAO
    {
        IEnumerable<MimicRotationDTO> LoadAll();

        IEnumerable<MimicRotationDTO> LoadByRotationType(MimicRotationType type);
    }
}
