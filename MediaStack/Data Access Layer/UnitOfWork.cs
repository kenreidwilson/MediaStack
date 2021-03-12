using MediaStack_Library.Model;
using Microsoft.EntityFrameworkCore;
using System;

namespace MediaStack_Library.Data_Access_Layer
{
    public class UnitOfWork<T> : IUnitOfWork where T : DbContext
    {
        private DbContext context;

        public IRepository<Media> Media { get; }

        public IRepository<Album> Albums { get; }

        public IRepository<Artist> Artists { get; }

        public IRepository<Category> Categories { get; }

        public IRepository<Tag> Tags { get; }

        public UnitOfWork()
        {
            this.context = (T)Activator.CreateInstance(typeof(T));
            this.Media = new Repository<Media>(this.context);
            this.Albums = new Repository<Album>(this.context);
            this.Artists = new Repository<Artist>(this.context);
            this.Categories = new Repository<Category>(this.context);
            this.Tags = new Repository<Tag>(this.context);
        }

        public void Save() => this.context.SaveChanges();
    }
}
