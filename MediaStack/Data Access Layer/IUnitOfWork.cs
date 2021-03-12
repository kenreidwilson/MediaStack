using MediaStack_Library.Model;

namespace MediaStack_Library.Data_Access_Layer
{
    public interface IUnitOfWork
    {
        IRepository<Media> Media { get; }

        IRepository<Album> Albums { get; }

        IRepository<Artist> Artists { get; }

        IRepository<Category> Categories { get; }

        IRepository<Tag> Tags { get; }

        public void Save();
    }
}
