using MediaStack_Library.Controllers;
using MediaStack_Library.Data_Access_Layer;

namespace MediaStack_Testing_Library.Fakes
{
    public class FakeMediaFSController : MediaFSController
    {
        public FakeMediaFSController(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {
            this.thumbnailer = new FakeThumbnailer();
        }
    }
}
