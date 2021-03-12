using MediaStack_Library.Model;
using MediaStack_Library.Utility;

namespace MediaStack_Testing_Library.Fakes
{
    public class FakeThumbnailer : Thumbnailer
    {
        public override bool CreateThumbnail(Media media)
        {
            return true;
        }
    }
}
