using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MediaStack_Library.Data_Access_Layer
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

        public virtual IQueryable<T> Get(Expression<Func<T, bool>> expression = null)
        {
            return this.context.Set<T>().Where(expression);
        }

        public virtual void Update(T entity)
        {
            try
            {
                this.context.Set<T>().Update(entity);
            }
            catch (InvalidOperationException)
            {
                // Return if the entity is tracked, therefore automatically updated.
                return;
            }
        }

        public virtual void Delete(T entity)
        {
            this.context.Set<T>().Remove(entity);
        }
    }
}
