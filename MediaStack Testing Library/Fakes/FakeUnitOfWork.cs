using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;

namespace MediaStack_Testing_Library.Fakes
{
    public class FakeUnitOfWork : IUnitOfWork
    {
        public IRepository<Media> Media { get; set; }
        public IRepository<Album> Albums { get; set; }
        public IRepository<Artist> Artists { get; set; }
        public IRepository<Category> Categories { get; set; }
        public IRepository<Tag> Tags { get; set; }

        public FakeUnitOfWork()
        {
            this.Media = new FakeRepository<Media>();
            this.Albums = new FakeRepository<Album>();
            this.Artists = new FakeRepository<Artist>();
            this.Categories = new FakeRepository<Category>();
            this.Tags = new FakeRepository<Tag>();
        }

        public void Save()
        {
            return;
        }
    }
}
