using System.Collections.Generic;

namespace MediaStack_Library.Model
{
    public class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public ICollection<Media> Media { get; set; } = new HashSet<Media>();
    }
}
