using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MediaStackCore.Services.HasherService
{
    public class SH1Hasher : IHasher
    {
        #region Properties

        private IDictionary<string, string> hashCache { get; } = new ConcurrentDictionary<string, string>();

        #endregion

        #region Methods

        public string CalculateHash(Stream stream, string cacheId = null)
        {
            if (cacheId != null && this.hashCache.ContainsKey(cacheId))
            {
                return this.hashCache[cacheId];
            }

            stream.Seek(0, SeekOrigin.Begin);
            using (var hasher = SHA1.Create())
            {
                byte[] hashBytes = hasher.ComputeHash(stream);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                if (cacheId != null)
                {
                    this.hashCache[cacheId] = hash;
                }

                return hash;
            }
        }

        public async Task<string> CalculateHashAsync(Stream stream, string cacheId = null)
        {
            if (cacheId != null && this.hashCache.ContainsKey(cacheId))
            {
                return this.hashCache[cacheId];
            }

            stream.Seek(0, SeekOrigin.Begin);
            using (var hasher = SHA1.Create())
            {
                byte[] hashBytes = await hasher.ComputeHashAsync(stream);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                if (cacheId != null)
                {
                    this.hashCache[cacheId] = hash;
                }

                return hash;
            }
        }

        public void ResetCache()
        {
            this.hashCache.Clear();
        }

        #endregion
    }
}