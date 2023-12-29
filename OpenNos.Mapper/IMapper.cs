using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Mapper
{
    public interface IMapper<TDto, TEntity> where TDto : class, new() where TEntity : class, new()
    {
        TEntity Map(TDto input);

        TDto Map(TEntity input);

        IEnumerable<TDto> Map(IEnumerable<TEntity> input);

        IEnumerable<TEntity> Map(IEnumerable<TDto> input);
    }
}
