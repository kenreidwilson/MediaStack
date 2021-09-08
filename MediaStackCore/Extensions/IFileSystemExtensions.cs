using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace MediaStackCore.Extensions
{
    public static class IFileSystemExtensions
    {
        #region Methods

        public static IEnumerable<IDirectoryInfo> GetDirectoriesAtLevel(this IFileSystem fs, string path,
            int targetLevel,
            int currentLevel = 0)
        {
            foreach (var currentDirectoryInfo in fs.DirectoryInfo.FromDirectoryName(path).GetDirectories().ToList())
            {
                if (currentLevel < targetLevel)
                {
                    foreach (IDirectoryInfo directoryInfo in GetDirectoriesAtLevel(fs, currentDirectoryInfo.FullName, targetLevel, currentLevel + 1))
                    {
                        yield return directoryInfo;
                    }
                }
                else
                {
                    yield return currentDirectoryInfo;
                }
            }
        }

        #endregion
    }
}
