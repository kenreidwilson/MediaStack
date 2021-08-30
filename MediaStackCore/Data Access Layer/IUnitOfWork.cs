using System;
using System.Threading.Tasks;
using MediaStackCore.Models;

namespace MediaStackCore.Data_Access_Layer
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Media> Media { get; }

        IRepository<Album> Albums { get; }

        IRepository<Artist> Artists { get; }

        IRepository<Category> Categories { get; }

        IRepository<Tag> Tags { get; }

        void Save();

        Task SaveAsync();
    }
}
