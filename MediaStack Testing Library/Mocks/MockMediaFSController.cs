using MediaStack_Library.Controllers;
using MediaStack_Library.Data_Access_Layer;

namespace MediaStack_Testing_Library.Mocks
{
    public class MockMediaFSController : MediaFSController
    {
        public MockMediaFSController(IUnitOfWork unitOfWork) : base() 
        {
            this.thumbnailer = new MockThumbnailer();
        }
    }
}
