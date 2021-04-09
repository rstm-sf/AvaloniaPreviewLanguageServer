using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace AvaloniaPreviewLanguageServer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DidChangeWorkspaceFoldersHandler : IDidChangeWorkspaceFoldersHandler
    {
        private readonly ProjectsManager _projectsManager;

        public DidChangeWorkspaceFoldersHandler(ProjectsManager projectsManager)
        {
            _projectsManager = projectsManager;
        }

        public async Task<Unit> Handle(DidChangeWorkspaceFoldersParams request, CancellationToken cancellationToken)
        {
            foreach (WorkspaceFolder workspaceFolder in request.Event.Added)
            {
                var pathFolder = workspaceFolder.Uri.GetFileSystemPath();
                var paths = FileSystemHelpers.FindCsprojFilesInDirectory(pathFolder);
                foreach (var path in paths)
                {
                    await _projectsManager.UpdateProjectAsync(path);
                }
            }

            return await Unit.Task;
        }

        public DidChangeWorkspaceFolderRegistrationOptions GetRegistrationOptions(ClientCapabilities clientCapabilities)
        {
            return new()
            {
                Supported = clientCapabilities.Workspace?.WorkspaceFolders == true,
                ChangeNotifications = clientCapabilities.Workspace?.WorkspaceFolders == true
            };
        }
    }
}
