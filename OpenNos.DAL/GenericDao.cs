using OpenNos.Core.Logger;
using OpenNos.DAL.EF.Helpers;
using OpenNos.Data.Enums;
using OpenNos.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenNos.DAL
{
    public class GenericDao<TEntity, TDto> : IGenericDao<TEntity, TDto> where TEntity : class, new() where TDto : class, new()
    {
        private readonly IMapper<TDto, TEntity> _mapper;

        private readonly PropertyInfo _primaryKey;

        public GenericDao(IMapper<TDto, TEntity> mapper)
        {
            _mapper = mapper;
            try
            {
                var pis = typeof(TDto).GetProperties();
                var exit = false;
                for (var index = 0; index < pis.Length && !exit; index++)
                {
                    var pi = pis[index];
                    var attrs = pi.GetCustomAttributes(typeof(KeyAttribute), false);
                    if (attrs.Length != 1)
                    {
                        continue;
                    }

                    exit = true;
                    _primaryKey = pi;
                }

                if (_primaryKey != null)
                {
                    return;
                }

                Logger.Log.Warn($"For DTO: {typeof(TDto).Name}:");
                throw new KeyNotFoundException();
            }
            catch (Exception e)
            {
                Logger.Log.Error(e.Message, e);
            }
        }

        public SaveResult Delete(object dtoKey)
        {
            using var context = DataAccessHelper.CreateContext();
            var dbSet = context.Set<TEntity>();

            if (dtoKey is IEnumerable enumerable)
            {
                foreach (var dto in enumerable)
                {
                    object value;
                    try
                    {
                        value = _primaryKey.GetValue(dto, null);
                    }
                    catch
                    {
                        value = dto;
                    }

                    TEntity foundEntity;
                    if (value is object[] objects)
                    {
                        foundEntity = dbSet.Find(objects);
                    }
                    else
                    {
                        foundEntity = dbSet.Find(value);
                    }

                    if (foundEntity == null)
                    {
                        continue;
                    }

                    dbSet.Remove(foundEntity);
                }
                context.SaveChanges();
            }
            else
            {
                object value;
                try
                {
                    value = _primaryKey.GetValue(dtoKey, null);
                }
                catch
                {
                    value = dtoKey;
                }

                var foundEntity = dbSet.Find(value);

                if (foundEntity != null)
                {
                    dbSet.Remove(foundEntity);
                }
            }

            context.SaveChanges();

            return SaveResult.Inserted;
        }

        public TDto FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                if (predicate == null)
                {
                    return default;
                }

                using var context = DataAccessHelper.CreateContext();
                var dbSet = context.Set<TEntity>();
                var ent = dbSet.FirstOrDefault(predicate);
                return _mapper.Map(ent);
            }
            catch (Exception e)
            {
                Logger.Log.Error(e.Message, e);
                return default;
            }
        }

        public TDto FirstOrDefault()
        {
            using var context = DataAccessHelper.CreateContext();
            var dbSet = context.Set<TEntity>();
            var entity = dbSet.FirstOrDefault();

            return _mapper.Map(entity);
        }

        public SaveResult InsertOrUpdate(ref TDto dto)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                var entity = _mapper.Map(dto);
                var dbSet = context.Set<TEntity>();

                var value = _primaryKey.GetValue(dto, null);
                TEntity foundEntity;
                if (value is object[] objects)
                {
                    foundEntity = dbSet.Find(objects);
                }
                else
                {
                    foundEntity = dbSet.Find(value);
                }

                entity = _mapper.Map(_mapper.Map(entity));
                if (foundEntity != null)
                {
                    context.Entry(foundEntity).CurrentValues.SetValues(entity);
                    context.SaveChanges();
                }

                if (value == null || foundEntity == null)
                {
                    dbSet.Add(entity);
                }

                context.SaveChanges();
                dto = _mapper.Map(entity);

                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(e.Message, e);
                return SaveResult.Error;
            }
        }

        public SaveResult InsertOrUpdate(IEnumerable<TDto> dtos)
        {
            try
            {
                using var context = DataAccessHelper.CreateContext();
                context.Configuration.AutoDetectChangesEnabled = false;

                var dbSet = context.Set<TEntity>();
                var entityToAdd = new List<TEntity>();
                foreach (var dto in dtos)
                {
                    var entity = _mapper.Map(dto);
                    var value = _primaryKey.GetValue(dto, null);

                    TEntity foundEntity;
                    if (value is object[] objects)
                    {
                        foundEntity = dbSet.Find(objects);
                    }
                    else
                    {
                        foundEntity = dbSet.Find(value);
                    }

                    if (foundEntity != null)
                    {
                        context.Entry(foundEntity).CurrentValues.SetValues(entity);
                    }

                    if (value == null || foundEntity == null)
                    {
                        entityToAdd.Add(entity);
                    }
                }

                dbSet.AddRange(entityToAdd);
                context.Configuration.AutoDetectChangesEnabled = true;
                context.SaveChanges();
                return SaveResult.Inserted;
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
                return SaveResult.Error;
            }
        }

        public IEnumerable<TDto> LoadAll()
        {
            using var context = DataAccessHelper.CreateContext();
            var dtoList = new List<TDto>();
            foreach (var entity in context.Set<TEntity>())
            {
                dtoList.Add(_mapper.Map(entity));
            }

            return dtoList;
        }

        public IEnumerable<TDto> Where(Expression<Func<TEntity, bool>> predicate)
        {
            using var context = DataAccessHelper.CreateContext();
            var dbSet = context.Set<TEntity>();
            var entities = Enumerable.Empty<TEntity>();
            try
            {
                entities = dbSet.Where(predicate).ToList();
            }
            catch (Exception e)
            {
                Logger.Log.Error(null, e);
            }

            var dtoList = new List<TDto>();
            foreach (var t in entities)
            {
                dtoList.Add(_mapper.Map(t));
            }

            return dtoList;
        }
    }
}
