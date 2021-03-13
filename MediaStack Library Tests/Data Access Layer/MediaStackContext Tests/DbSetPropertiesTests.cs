using MediaStack_Library.Model;
using MediaStack_Testing_Library.Mocks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MediaStack_Library_Tests.Data_Access_Layer.MediaStackContext_Tests
{
    public class DbSetPropertiesTests
    {
        private readonly MockMediaStackContext context;

        public DbSetPropertiesTests()
        {
            this.context = new MockMediaStackContext();
        }

        [Fact]
        public void Media_GetMedia_ReturnsDbSetOfMedia()
        {
            Assert.IsAssignableFrom<DbSet<Media>>(this.context.Media);
        }

        [Fact]
        public void Media_GetAlbums_ReturnsDbSetOfAlbums()
        {
            Assert.IsAssignableFrom<DbSet<Album>>(this.context.Albums);
        }

        [Fact]
        public void Media_GetCategories_ReturnsDbSetOfCategories()
        {
            Assert.IsAssignableFrom<DbSet<Category>>(this.context.Categories);
        }

        [Fact]
        public void Media_GetArtists_ReturnsDbSetOfArtists()
        {
            Assert.IsAssignableFrom<DbSet<Artist>>(this.context.Artists);
        }

        [Fact]
        public void Media_GetTags_ReturnsDbSetOfTags()
        {
            Assert.IsAssignableFrom<DbSet<Tag>>(this.context.Tags);
        }
    }
}
