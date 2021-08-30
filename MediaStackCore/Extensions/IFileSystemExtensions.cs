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
            var directoryNames = new List<IDirectoryInfo>();

            foreach (var item in fs.DirectoryInfo.FromDirectoryName(path).GetDirectories().ToList())
            {
                if (currentLevel < targetLevel)
                {
                    directoryNames.AddRange(GetDirectoriesAtLevel(fs, item.FullName, targetLevel, currentLevel + 1));
                }
                else
                {
                    directoryNames.Add(item);
                }
            }

            return directoryNames;
        }

        #endregion
    }
}
