using System.Linq;
using MediaStackCore.Data_Access_Layer;
using MediaStackCore.Models;

namespace MediaStack_API.Models.Requests
{
    public class TagSearchQuery
    {
        #region Properties

        public int Count { get; set; } = 5;

        public int Offset { get; set; } = 0;

        public string FuzzyName { get; set; }

        #endregion

        #region Methods

        public IQueryable<Tag> GetQuery(IUnitOfWork unitOfWork)
        {
            var query = unitOfWork.Tags.Get();

            if (this.FuzzyName != null)
            {
                query = query.Where(t => t.Name.Contains(this.FuzzyName));
            }

            return query;
        }

        #endregion
    }
}