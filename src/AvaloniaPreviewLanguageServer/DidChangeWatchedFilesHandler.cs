using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace AvaloniaPreviewLanguageServer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DidChangeWatchedFilesHandler : IDidChangeWatchedFilesHandler
    {
        private readonly ProjectsManager _projectManager;

        public DidChangeWatchedFilesHandler(ProjectsManager projectsManager)
        {
            _projectManager = projectsManager;
        }

        public async Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
        {
            foreach (var change in request.Changes)
            {
                if (change.Type == FileChangeType.Deleted)
                {
                    continue;
                }

                var filePath = change.Uri.GetFileSystemPath();
                await _projectManager.UpdateProjectAsync(filePath);
            }

            return await Unit.Task;
        }

        public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions(
            DidChangeWatchedFilesCapability capability, ClientCapabilities clientCapabilities)
        {
            return new();
        }
    }
}
