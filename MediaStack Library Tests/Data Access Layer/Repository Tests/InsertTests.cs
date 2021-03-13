using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using MediaStack_Testing_Library.Mocks;
using System.Linq;
using Xunit;

namespace MediaStack_Library_Tests.Data_Access_Layer.Repository_Tests
{
    public class InsertTests
    {
        private readonly MockMediaStackContext context;
        private readonly Repository<Category> entityRepository;

        public InsertTests()
        {
            this.context = new MockMediaStackContext();
            this.entityRepository = new Repository<Category>(this.context);
        }

        [Fact]
        public void Repository_Insert_InsertsEntity()
        {
            Category entity = new Category { Id = 1, Name = "Test Category" };
            this.entityRepository.Insert(entity);
            this.context.SaveChanges();
            Assert.Equal(entity, this.entityRepository.Get().First());
        }
    }
}
