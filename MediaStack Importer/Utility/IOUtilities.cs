using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MediaStack_Importer.Utility
{
    public static class IOUtilities
    {
        #region Methods

        public static IEnumerable<DirectoryInfo> GetDirectoriesAtLevel(string path, int targetLevel, int currentLevel = 0)
        {
            var directoryNames = new List<DirectoryInfo>();

            foreach (var item in new DirectoryInfo(path).GetDirectories().ToList())
            {
                if (currentLevel < targetLevel)
                {
                    directoryNames.AddRange(GetDirectoriesAtLevel(item.FullName, targetLevel, currentLevel + 1));
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