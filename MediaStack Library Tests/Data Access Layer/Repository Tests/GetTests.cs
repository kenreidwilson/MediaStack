using MediaStack_Library.Data_Access_Layer;
using MediaStack_Library.Model;
using MediaStack_Testing_Library.Mocks;
using System.Linq;
using Xunit;

namespace MediaStack_Library_Tests.Data_Access_Layer.Repository_Tests
{
    public class GetTests
    {
        private readonly MockMediaStackContext context;
        private readonly Repository<Category> entityRepository;

        public GetTests()
        {
            this.context = new MockMediaStackContext();
            this.entityRepository = new Repository<Category>(this.context);
        }

        [Fact]
        public void Repository_Get_ReturnsIQueryableOfEntity()
        {
            Assert.IsAssignableFrom<IQueryable<Category>>(this.entityRepository.Get());
        }

        [Fact]
        public void Repository_Get_ReturnIsEmpty()
        {
            Assert.Empty(this.entityRepository.Get());
        }

        [Fact]
        public void Repository_Get_ReturnsEmptyWithExpression()
        {
            Assert.Empty(this.entityRepository.Get().Where(c => c.Name == "test"));
        }
    }
}
