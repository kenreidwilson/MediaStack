using System.Collections;
using System.Collections.Generic;

namespace MediaStack_Library.Model
{
    public class Tag
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public virtual ICollection<Media> Media { get; set; } = new HashSet<Media>();
    }
}
