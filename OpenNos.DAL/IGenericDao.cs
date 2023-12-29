using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data.Enums;

namespace OpenNos.DAL
{
    public interface IGenericDao<TEntity, TDto>
    {
        /// <summary>
        /// Deletes an object from the database based on the [Key] Property
        /// </summary>
        /// <param name="dtoKey"></param>
        /// <returns></returns>
        SaveResult Delete(object dtoKey);

        /// <summary>
        /// Finds the first element matching the predicate expression
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        TDto FirstOrDefault(Expression<Func<TEntity, bool>> predicate);


        /// <summary>
        /// Returns the first element found in the context
        /// </summary>
        /// <returns></returns>
        TDto FirstOrDefault();

        /// <summary>
        /// Inserts or updates a DTO passed as reference
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        SaveResult InsertOrUpdate(ref TDto dto);

        /// <summary>
        /// Inserts or updates a collection of DTOs
        /// </summary>
        /// <param name="dtoList"></param>
        /// <returns></returns>
        SaveResult InsertOrUpdate(IEnumerable<TDto> dtoList);

        /// <summary>
        /// Loads all the data in the given context
        /// </summary>
        /// <returns></returns>
        IEnumerable<TDto> LoadAll();

        /// <summary>
        /// Returns an IEnumerable matching the predicate expression
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IEnumerable<TDto> Where(Expression<Func<TEntity, bool>> predicate);
    }
}
