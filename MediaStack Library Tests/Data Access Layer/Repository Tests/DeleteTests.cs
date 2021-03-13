using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using MediaStack_Testing_Library.Mocks;
using System.Linq;
using Xunit;

namespace MediaStack_Library_Tests.Data_Access_Layer.Repository_Tests
{
    public class DeleteTests
    {
        private readonly MockMediaStackContext context;
        private readonly Repository<Category> entityRepository;

        public DeleteTests()
        {
            this.context = new MockMediaStackContext();
            this.entityRepository = new Repository<Category>(this.context);
            this.entityRepository.Insert(new Category { ID = 1, Name = "Test Category" });
            this.context.SaveChanges();
        }

        [Fact]
        public void Repository_Delete_DeletesEntity()
        {
            var entity = this.entityRepository.Get().First();
            this.entityRepository.Delete(entity);
            this.context.SaveChanges();
            Assert.Empty(this.entityRepository.Get());
        }
    }
}
