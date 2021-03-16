using MediaStack_Library.Data_Access_Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MediaStack_Testing_Library.Fakes
{
    public class FakeRepository<T> : IRepository<T> where T : class
    {
        #region Properties

        public List<T> items { get; set; }

        #endregion

        #region Constructors

        public FakeRepository()
        {
            this.items = new List<T>();
        }

        #endregion

        #region Methods

        public void Delete(T entity)
        {
            this.items.Remove(entity);
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> expression = null)
        {
            return expression == null ? this.items.AsQueryable() : this.items.AsQueryable().Where(expression);
        }

        public void Insert(T entity)
        {
            this.items.Add(entity);
        }

        public void Update(T entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> GetLocal(Expression<Func<T, bool>> expression = null)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
