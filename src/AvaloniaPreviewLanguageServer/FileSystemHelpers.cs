using System;
using System.Collections.Generic;
using System.IO;

namespace AvaloniaPreviewLanguageServer
{
    internal static class FileSystemHelpers
    {
        public static IReadOnlyList<string> FindCsprojFilesInDirectory(string directoryPath)
        {
            var result = new List<string>();
            foreach (var path in Directory.GetFiles(directoryPath, "*.*"))
            {
                var isProject = path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                             || path.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase);
                if (!isProject)
                {
                    continue;
                }

                result.Add(path);
            }

            foreach (var path in Directory.GetDirectories(directoryPath))
            {
                var shouldSkip = path.Equals("bin", StringComparison.OrdinalIgnoreCase)
                              || path.Equals("obj", StringComparison.OrdinalIgnoreCase);
                if (shouldSkip)
                {
                    continue;
                }

                result.AddRange(FindCsprojFilesInDirectory(path));
            }

            return result;
        }
    }
}
