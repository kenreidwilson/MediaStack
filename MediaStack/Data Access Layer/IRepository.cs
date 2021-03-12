using System;
using System.Linq;
using System.Linq.Expressions;

namespace MediaStack_Library.Data_Access_Layer
{
    public interface IRepository<T> where T : class
    {
        void Insert(T entity);

        IQueryable<T> Get(Expression<Func<T, bool>> expression = null);

        void Update(T entity);

        void Delete(T entity);
    }
}
