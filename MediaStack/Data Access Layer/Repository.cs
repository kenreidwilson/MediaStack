using System;
using System.Linq;
using System.Linq.Expressions;
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

        public virtual IQueryable<T> Get(Expression<Func<T, bool>> expression = null)
        {
            return expression == null ? this.context.Set<T>() : this.context.Set<T>().Where(expression);
        }

        public virtual IQueryable<T> GetLocal(Expression<Func<T, bool>> expression = null)
        {
            return expression == null ? this.context.Set<T>().Local.AsQueryable() : this.context.Set<T>().Local.AsQueryable().Where(expression);
        }

        public virtual void Update(T entity)
        {
            this.context.Set<T>().Update(entity);
        }

        public virtual void Delete(T entity)
        {
            this.context.Set<T>().Remove(entity);
        }
    }
}
