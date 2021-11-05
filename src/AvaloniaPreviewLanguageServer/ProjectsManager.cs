using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaProjectInfoResolver;

namespace AvaloniaPreviewLanguageServer
{
    public record PreviewParameters(
        string AvaloniaPreviewPath,
        string TargetPath,
        string ProjectDepsFilePath,
        string ProjectRuntimeConfigFilePath,
        string XamlFilePath = "");

    public class ProjectsManager
    {
        private readonly ConcurrentDictionary<string, (string[] resourcePaths, PreviewParameters previewParameters)>
            _buffers
                = new(StringComparer.OrdinalIgnoreCase);

        private readonly IProjectInfoResolver _resolver = new ProjectInfoResolver();

        public async Task UpdateProjectAsync(string projectPath)
        {
            projectPath = projectPath.Replace('\\', '/');
            var result = await _resolver.ResolvePreviewProjectInfoAsync(projectPath);
            if (result.HasError)
            {
                return;
            }

            var directoryPath = GetDirectoryPath(projectPath);

            var projectInfo = result.ProjectInfo!;
            var xamlResourcesFilePaths = projectInfo.AvaloniaResource
                .Split(';')
                .Select(x => GetResourcePath(directoryPath, x))
                .ToList();

            xamlResourcesFilePaths.AddRange(
                projectInfo.AvaloniaXaml
                    .Split(';')
                    .Select(x => GetResourcePath(directoryPath, x)));

            var previewParameters = new PreviewParameters(
                projectInfo.AvaloniaPreviewerNetCoreToolPath,
                projectInfo.ProjectInfoByTfmArray[0].TargetPath,
                projectInfo.ProjectInfoByTfmArray[0].ProjectDepsFilePath,
                projectInfo.ProjectInfoByTfmArray[0].ProjectRuntimeConfigFilePath);

            var value = (xamlResourcesFilePaths.ToArray(), previewParameters);
            _buffers.AddOrUpdate(projectPath, value, (_, _) => value);
        }

        public bool TryGetPreviewParameters(string filePath, [NotNullWhen(true)] out PreviewParameters? parameters)
        {
            parameters = default;
            filePath = filePath.Replace('\\', '/');
            foreach (var (_, (resourcePaths, previewParameters)) in _buffers)
            {
                var isContains = resourcePaths.Any(x => x.Equals(filePath, StringComparison.OrdinalIgnoreCase));
                if (!isContains)
                {
                    continue;
                }

                parameters = previewParameters with {XamlFilePath = filePath};
                return true;
            }

            return false;
        }

        private static string GetDirectoryPath(string path)
        {
            var span = path.AsSpan();
            for (var i = span.Length - 1; i >= 0; --i)
            {
                if (span[i] == '/')
                {
                    return span[..i].ToString();
                }
            }

            return string.Empty;
        }

        private static string GetResourcePath(string directoryPath, string relFilePath) =>
            new FileInfo(Path.Combine(directoryPath, relFilePath)).FullName.Replace('\\', '/');
    }
}
