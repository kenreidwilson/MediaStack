using System.Collections.Generic;

namespace MediaStack_Library.Model
{
    public class Album
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int ArtistID { get; set; }

        public Artist Artist { get; set; }

        public ICollection<Media> Media { get; set; } = new HashSet<Media>();
    }
}
