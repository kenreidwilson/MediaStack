using System.Threading.Tasks;
using MediaStackCore.Models;
using Microsoft.EntityFrameworkCore;

namespace MediaStackCore.Data_Access_Layer
{
    public class UnitOfWork : IUnitOfWork
    {
        protected DbContext Context;

        public virtual IRepository<Media> Media { get; }

        public virtual IRepository<Album> Albums { get; }

        public virtual IRepository<Artist> Artists { get; }

        public virtual IRepository<Category> Categories { get; }

        public virtual IRepository<Tag> Tags { get; }

        public UnitOfWork(DbContext context)
        {
            this.Context = context;
            this.Media = new Repository<Media>(this.Context);
            this.Albums = new Repository<Album>(this.Context);
            this.Artists = new Repository<Artist>(this.Context);
            this.Categories = new Repository<Category>(this.Context);
            this.Tags = new Repository<Tag>(this.Context);
        }

        public virtual void Save()
        {
            this.Context.SaveChanges();
        }

        public virtual async Task SaveAsync()
        {
            await this.Context.SaveChangesAsync();
        }

        public virtual void Dispose()
        {
            this.Context.Dispose();
        }
    }
}
