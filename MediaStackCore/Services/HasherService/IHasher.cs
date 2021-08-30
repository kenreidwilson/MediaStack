using System.IO;
using System.Threading.Tasks;

namespace MediaStackCore.Services.HasherService
{
    public interface IHasher
    {
        #region Methods

        /// <summary>
        ///     Calculates and returns a string representation of the hashed file stream.
        /// </summary>
        /// <param name="stream">A FileStream</param>
        /// <param name="cacheId"></param>
        /// <returns>a string representation of the hashed file stream</returns>
        public string CalculateHash(Stream stream, string cacheId = null);

        public Task<string> CalculateHashAsync(Stream stream, string cacheId = null);

        public void ResetCache();

        #endregion
    }
}