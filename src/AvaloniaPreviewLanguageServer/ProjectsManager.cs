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
            if (_buffers.ContainsKey(projectPath))
                return;

            var result = await _resolver.ResolvePreviewInfoAsync(projectPath);
            if (result.HasError)
            {
                return;
            }

            AddPreviewInfoToBuffer(result.PreviewInfo!, result.PreviewInfo!.XamlFileInfo);
            foreach (var xamlFileInfo in result.PreviewInfo.XamlFileInfo.ReferenceXamlFileInfoCollection)
                AddPreviewInfoToBuffer(result.PreviewInfo!, xamlFileInfo);
        }

        private void AddPreviewInfoToBuffer(PreviewInfo previewInfo, XamlFileInfo xamlFileInfo)
        {
            var directoryPath = Path.GetDirectoryName(xamlFileInfo.ProjectPath)!;

            var xamlResourcesFilePaths = xamlFileInfo.AvaloniaResource
                .Split(';')
                .Select(x => GetResourcePath(directoryPath, x))
                .ToList();

            xamlResourcesFilePaths.AddRange(
                xamlFileInfo.AvaloniaXaml
                    .Split(';')
                    .Select(x => GetResourcePath(directoryPath, x)));

            var previewParameters = new PreviewParameters(
                previewInfo.AvaloniaPreviewerNetCoreToolPath,
                previewInfo.AppExecInfoCollection[0].TargetPath,
                previewInfo.AppExecInfoCollection[0].ProjectDepsFilePath,
                previewInfo.AppExecInfoCollection[0].ProjectRuntimeConfigFilePath);

            var value = (xamlResourcesFilePaths.ToArray(), previewParameters);
            var path = xamlFileInfo.ProjectPath.Replace('\\', '/');
            _buffers.AddOrUpdate(path, value, (_, _) => value);
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

        private static string GetResourcePath(string directoryPath, string relFilePath) =>
            new FileInfo(Path.Combine(directoryPath, relFilePath)).FullName.Replace('\\', '/');
    }
}
