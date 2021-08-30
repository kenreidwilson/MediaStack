using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MediaStackCore.Data_Access_Layer
{
    public class Repository<T> : IRepository<T> where T : class    {
        protected readonly DbContext context;

        public Repository(DbContext context)
        {
            this.context = context;
        }

        public virtual void Insert(T entity)
        {
            this.context.Set<T>().Add(entity);
        }

        public virtual async Task InsertAsync(T entity)
        {
            await this.context.Set<T>().AddAsync(entity);
        }

        public virtual void BulkInsert(IList<T> entities)
        {
            this.context.Set<T>().AddRange(entities);
        }

        public virtual async Task BulkInsertAsync(IList<T> entities)
        {
            await this.context.Set<T>().AddRangeAsync(entities);
        }

        public virtual IQueryable<T> Get(Expression<Func<T, bool>> expression = null)
        {
            return expression == null ? this.context.Set<T>() : this.context.Set<T>().Where(expression);
        }

        public virtual void Update(T entity)
        {
            this.context.Set<T>().Update(entity);
        }

        public virtual void BulkUpdate(IList<T> entities)
        {
            this.context.UpdateRange(entities);
        }

        public virtual async Task BulkUpdateAsync(IList<T> entities)
        {
            await this.context.Set<T>().AddRangeAsync(entities);
        }

        public virtual void Delete(T entity)
        {
            this.context.Set<T>().Remove(entity);
        }
    }
}
