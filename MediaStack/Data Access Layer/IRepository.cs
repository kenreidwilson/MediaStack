using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MediaStackCore.Data_Access_Layer
{
    public interface IRepository<T> where T : class
    {
        void Insert(T entity);

        Task InsertAsync(T entity);

        void BulkInsert(IList<T> entities);

        Task BulkInsertAsync(IList<T> entities);

        IQueryable<T> Get(Expression<Func<T, bool>> expression = null);

        void Update(T entity);

        void BulkUpdate(IList<T> entities);

        Task BulkUpdateAsync(IList<T> entities);

        void Delete(T entity);
    }
}
