using MediaStack_Library.Model;
using MediaStack_Library.Utility;

namespace MediaStack_Testing_Library.Mocks
{
    public class MockThumbnailer : Thumbnailer
    {
        public override bool CreateThumbnail(Media media)
        {
            return true;
        }
    }
}
