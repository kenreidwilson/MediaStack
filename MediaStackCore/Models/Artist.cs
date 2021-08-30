using System.Collections.Generic;

namespace MediaStackCore.Models
{
    public class Artist
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public ICollection<Media> Media { get; set; } = new HashSet<Media>();


    }
}
