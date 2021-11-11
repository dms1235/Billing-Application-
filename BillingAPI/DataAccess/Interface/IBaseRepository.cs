using Entity.Base;
using Entity.Common.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IBaseRepository<T> where T : BaseEntity, new()
    {
        Task<ResultEntity<T>> Insert(T entity);
        Task<ResultList<T>> InsertAll(List<T> entity);
        Task<ResultEntity<T>> Update(T entity);
        Task<ResultEntity<T>> Delete(T entity);
        Task<ResultEntity<T>> FindOneByQuery(Expression<Func<T, bool>> expression);
        Task<ResultList<T>> FindByQuery(Expression<Func<T, bool>> expression);
        Task<ResultList<T>> FindAllItems();
        Task<ResultList<T>> FindAll(int pageIndex, int pageSize);
        Task<ResultEntity<Int32>> FindCountByQuery(Expression<Func<T, bool>> expression);
        Task<ResultEntity<Int32>> FindCount();

        Task<ResultList<T>> FindAll(GridParameters gridParams, Expression<Func<T, bool>> expression = null);
    }
}
