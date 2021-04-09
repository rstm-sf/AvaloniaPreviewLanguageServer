using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;

namespace AvaloniaPreviewLanguageServer
{
    internal static class Program
    {
        private static async Task Main()
        {
            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .WithLoggerFactory(new LoggerFactory())
                    .AddDefaultLoggingProvider()
                    .WithServices(ConfigureServices)
                    .WithHandler<TextDocumentSyncHandler>()
                    .WithHandler<DidChangeWorkspaceFoldersHandler>()
                    .WithHandler<DidChangeWatchedFilesHandler>()
                    .OnStarted(async (languageServer, _) =>
                    {
                        var folders = await languageServer.WorkspaceFolderManager
                            .Refresh()
                            .ToArray();
                        if (folders.Length == 0)
                        {
                            return;
                        }

                        var manager = languageServer.GetService<ProjectsManager>();
                        foreach (var folder in folders)
                        {
                            var folderPath = folder.Uri.GetFileSystemPath();
                            var paths = FileSystemHelpers.FindCsprojFilesInDirectory(folderPath);
                            foreach (var path in paths)
                            {
                                await manager.UpdateProjectAsync(path);
                            }
                        }
                    }));

            await server.WaitForExit;
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ProjectsManager>();
        }
    }
}
