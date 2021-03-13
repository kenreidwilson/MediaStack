using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using MediaStack_Testing_Library.Mocks;
using System.Linq;
using Xunit;

namespace MediaStack_Library_Tests.Data_Access_Layer.Repository_Tests
{
    public class UpdateTests
    {
        private readonly MockMediaStackContext context;
        private readonly Repository<Category> entityRepository;

        public UpdateTests()
        {
            this.context = new MockMediaStackContext();
            this.entityRepository = new Repository<Category>(this.context);
            this.entityRepository.Insert(new Category { Id = 1, Name = "Test Category" });
            this.context.SaveChanges();
        }

        [Fact]
        public void Repository_Update_UpdatesEntity()
        {
            var entity = this.entityRepository.Get().First();
            entity.Name = "Updated Test Category";
            this.entityRepository.Update(entity);
            this.context.SaveChanges();
            entity = this.entityRepository.Get().First();
            Assert.Equal("Updated Test Category", entity.Name);
        }

        [Fact]
        public void Repository_Update_UpdatesTwiceWithNoExceptionThrown()
        {
            var entity = this.entityRepository.Get().First();
            entity.Name = "Updated Test Category";
            this.entityRepository.Update(entity);
            this.context.SaveChanges();
            this.entityRepository.Update(entity);
            this.context.SaveChanges();
            entity = this.entityRepository.Get().First();
            Assert.Equal("Updated Test Category", entity.Name);
        }
    }
}
