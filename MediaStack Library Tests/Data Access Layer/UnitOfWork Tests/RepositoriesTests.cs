using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using MediaStack_Testing_Library.Mocks;
using Xunit;

namespace MediaStack_Library_Tests.Data_Access_Layer.UnitOfWork_Tests
{
    public class RepositoriesTests
    {
        #region Data members

        protected readonly IUnitOfWork unitOfWork;

        #endregion

        #region Constructors

        public RepositoriesTests()
        {
            this.unitOfWork = new UnitOfWork<MockMediaStackContext>();
        }

        #endregion

        [Fact]
        public void Terms_GetMedia_ReturnsIRepositoryOfMedia()
        {
            Assert.IsAssignableFrom<IRepository<Media>>(unitOfWork.Media);
        }

        [Fact]
        public void Terms_GetAlbums_ReturnsIRepositoryOfAlbum()
        {
            Assert.IsAssignableFrom<IRepository<Album>>(unitOfWork.Albums);
        }

        [Fact]
        public void Terms_GetArtists_ReturnsIRepositoryOfArtist()
        {
            Assert.IsAssignableFrom<IRepository<Artist>>(unitOfWork.Artists);
        }

        [Fact]
        public void Terms_GetCategories_ReturnsIRepositoryOfCategory()
        {
            Assert.IsAssignableFrom<IRepository<Category>>(unitOfWork.Categories);
        }

        [Fact]
        public void Terms_GetTags_ReturnsIRepositoryOfTag()
        {
            Assert.IsAssignableFrom<IRepository<Tag>>(unitOfWork.Tags);
        }

    }
}
