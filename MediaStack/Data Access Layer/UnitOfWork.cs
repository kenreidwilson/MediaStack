using MediaStackCore.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaStackCore.Data_Access_Layer
{
    public class UnitOfWork : IUnitOfWork
    {
        protected DbContext Context;

        public IRepository<Media> Media { get; }

        public IRepository<Album> Albums { get; }

        public IRepository<Artist> Artists { get; }

        public IRepository<Category> Categories { get; }

        public IRepository<Tag> Tags { get; }

        public UnitOfWork(DbContext context)
        {
            this.Context = context;
            this.Media = new Repository<Media>(this.Context);
            this.Albums = new Repository<Album>(this.Context);
            this.Artists = new Repository<Artist>(this.Context);
            this.Categories = new Repository<Category>(this.Context);
            this.Tags = new Repository<Tag>(this.Context);
        }

        public void Save()
        {
            this.Context.SaveChanges();
        }

        public void Dispose()
        {
            this.Context.Dispose();
        }
    }
}
