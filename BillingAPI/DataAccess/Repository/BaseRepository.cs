using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using DataAccess.Common;
using DataAccess.Interface;
using Entity.Base;
using Entity.Common.Constants;
using Entity.Common.Entities;
using Entity.Common.Helper;
using Entity.Common.Logs;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity, new()
    {
        ApplicationDBContext dbContext;
        public BaseRepository(ApplicationDBContext _dbContext)
        {
            dbContext = _dbContext;
        }
        public async Task<ResultEntity<T>> Delete(T entity)
        {
            ResultEntity<T> result = new ResultEntity<T>();
            try
            {
                dbContext.Entry(entity).State = EntityState.Deleted;
                await dbContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.DeleteSuccessMessage;
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultList<T>> FindAll(int pageIndex, int pageSize)
        {
            ResultList<T> result = new ResultList<T>();
            try
            {
                var count = await dbContext.Set<T>().CountAsync();
                if (count > 0)
                {
                    result.ItemCount = count;
                    result.TotalPages = (int)Math.Ceiling((double)count / pageSize);
                    var skip = (pageIndex - 1) * pageSize;
                    var data = await dbContext.Set<T>().Skip(skip).Take(pageSize).OrderBy(a => a.CreatedOn).ToListAsync();
                    if (data.Count > 0)
                        result.List = data;
                    else
                    {
                        result.Status = 3;
                        result.MessageEnglish = CommonRepositoryMessages.CannotFindAllMessage;
                        
                    }
                }
                else
                {
                    result.Status = 3;
                    result.MessageEnglish =  CommonRepositoryMessages.CannotFindAllMessage;
                }
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultList<T>> FindAllItems()
        {
            ResultList<T> result = new ResultList<T>();
            try
            {
                var data = await dbContext.Set<T>().OrderBy(a => a.CreatedOn).ToListAsync();
                if (data.Count > 0)
                    result.List = data;
                else
                {
                    result.Status = 3;
                    result.MessageEnglish =  CommonRepositoryMessages.CannotFindAllMessage;
                }
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =  CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultEntity<T>> FindOneByQuery(Expression<Func<T, bool>> expression)
        {
            ResultEntity<T> result = new ResultEntity<T>();
            try
            {
                var data = await dbContext.Set<T>().AsNoTracking().Where(expression).OrderByDescending(a => a.CreatedOn).FirstOrDefaultAsync();
                if (data != null)
                {
                    result.Entity = data;
                }
                else
                {
                    result.Status = 3;
                    result.Entity = null;
                    result.MessageEnglish = CommonRepositoryMessages.CannotFindByIDMessage;
                }
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultList<T>> FindByQuery(Expression<Func<T, bool>> expression)
        {
            ResultList<T> result = new ResultList<T>();
            try
            {
                var data = await dbContext.Set<T>().Where(expression).OrderBy(a => a.CreatedOn).ToListAsync();
                if (data.Count() > 0)
                {
                    result.List = data;
                }
                else
                {
                    result.Status = 3;
                    result.MessageEnglish = CommonRepositoryMessages.CannotFindByIDMessage;
                  
                }
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultEntity<T>> Insert(T entity)
        {
            ResultEntity<T> result = new ResultEntity<T>();
            try
            {
                dbContext.Set<T>().Add(entity);
                await dbContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.InsertSuccessMessage;
             
                result.Entity = entity;
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }
        public async Task<ResultList<T>> InsertAll(List<T> entity)
        {
            ResultList<T> result = new ResultList<T>();
            try
            {
                dbContext.Set<T>().AddRange(entity);
                await dbContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.InsertSuccessMessage;
                
                result.List = entity;
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish = CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultEntity<T>> Update(T entity)
        {
            ResultEntity<T> result = new ResultEntity<T>();
            try
            {
                //  dbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                dbContext.Entry(entity).CurrentValues.SetValues(entity);
                dbContext.Entry(entity).State = EntityState.Modified;
                dbContext.Entry(entity).Property(p => p.CreatedBy).IsModified = false;
                dbContext.Entry(entity).Property(p => p.CreatedOn).IsModified = false;
                await dbContext.SaveChangesAsync();
                result.MessageEnglish = CommonRepositoryMessages.UpdateSuccessMessage;
             
                result.Entity = entity;
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =  CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultEntity<int>> FindCountByQuery(Expression<Func<T, bool>> expression)
        {
            ResultEntity<int> result = new ResultEntity<int>();
            try
            {
                var data = await dbContext.Set<T>().Where(expression).CountAsync();
                result.Entity = data;
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultEntity<int>> FindCount()
        {
            ResultEntity<int> result = new ResultEntity<int>();
            try
            {
                var data = await dbContext.Set<T>().CountAsync();
                result.Entity = data;
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

        public async Task<ResultList<T>> FindAll(GridParameters gridParams, Expression<Func<T, bool>> expression = null)
        {
            ResultList<T> result = new ResultList<T>();
            try
            {
                if (expression == null)
                {
                    expression = x => true;
                }

                var count = await dbContext.Set<T>().Where(expression).CountAsync();
                if (count > 0)
                {
                    if (gridParams.PageNumber == 0)
                    {
                        gridParams.PageNumber = 1;
                    }

                    if (gridParams.PageSize == 0)
                    {
                        gridParams.PageSize = count;
                        gridParams.PageNumber = 1;
                    }
                    gridParams.SortBy = string.IsNullOrWhiteSpace(gridParams.SortBy) ? "CreatedOn" : gridParams.SortBy;
                    result.ItemCount = count;
                    result.TotalPages = (int)Math.Ceiling((double)count / gridParams.PageSize);
                    var skip = (gridParams.PageNumber - 1) * gridParams.PageSize;

                    var data = await dbContext.Set<T>().Where(expression).SortBy(gridParams.SortBy, gridParams.IsAscending, gridParams.Culture)
                        .Skip(skip).Take(gridParams.PageSize).ToListAsync();

                    if (data.Count > 0)
                        result.List = data;
                    else
                    {
                        result.Status = 3;
                        result.MessageEnglish =  CommonRepositoryMessages.CannotFindAllMessage;
                    }
                }
                else
                {
                    result.Status = 3;
                    result.MessageEnglish =   CommonRepositoryMessages.CannotFindAllMessage;
                }
            }
            catch (Exception ex)
            {
                result.Status = 1;
                result.MessageEnglish =   CommonRepositoryMessages.ExceptionMessage;
                result.DetailsEnglish = ex.Message + Environment.NewLine + ex.StackTrace;
                LogHelper.LogError(ex, ex.ToMessageString());
            }
            return result;
        }

    }
}
