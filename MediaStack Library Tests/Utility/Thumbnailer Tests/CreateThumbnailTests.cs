using MediaStack_Library.Model;
using MediaStack_Library.Utility;
using System.IO;
using Xunit;

namespace MediaStack_Library_Tests.Utility.Thumbnailer_Tests
{
    public class CreateThumbnailTests
    {
        private readonly Thumbnailer thumbnailer;

        public CreateThumbnailTests()
        {
            this.thumbnailer = new Thumbnailer();
        }

        [Fact]
        public void Thumbnailer_CreateThumbnail_FalseWhenMediaIsNew()
        {
            Assert.False(this.thumbnailer.CreateThumbnail(new Media()));
        }

        [Fact]
        public void Thumbnailer_CreateThumbnail_FalseWhenImageDoesNotExist()
        {
            Assert.False(this.thumbnailer.CreateThumbnail(new Media { Type = MediaType.Image }));
            Assert.False(this.thumbnailer.CreateThumbnail(new Media { Type = MediaType.Animated_Image }));
            Assert.False(this.thumbnailer.CreateThumbnail(new Media { Type = MediaType.Video }));
        }

        [Fact]
        public void Thumbnailer_CreateThumbnail_TrueWhenImageThumbnailIsCreated()
        {
            Assert.True(this.thumbnailer.CreateThumbnail(new Media { Type = MediaType.Image, Path = @$"Test Files{Path.DirectorySeparatorChar}input_artist{Path.DirectorySeparatorChar}image_3.jpg" }));
        }
    }
}
