using System.Linq;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_API.Models.Requests
{
    public class ArtistSearchQuery
    {
        public int Count { get; set; } = 5;

        public int Offset { get; set; } = 0;

        public string FuzzyName { get; set; }

        public IQueryable<Artist> GetQuery(IUnitOfWork unitOfWork)
        {
            var query = unitOfWork.Artists.Get();

            if (this.FuzzyName != null) query = query.Where(a => a.Name.Contains(this.FuzzyName));

            return query;
        }
    }
}
