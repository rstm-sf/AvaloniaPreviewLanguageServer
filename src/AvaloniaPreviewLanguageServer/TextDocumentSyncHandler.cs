using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace AvaloniaPreviewLanguageServer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TextDocumentSyncHandler : TextDocumentSyncHandlerBase
    {
        private static readonly string HtmlUrl = $"http://127.0.0.1:{IpUtilities.GetAvailablePort()}";

        private readonly ProjectsManager _projectsManager;
        private readonly ILanguageServerFacade _router;
        private Process? _htmlPreviewerProcess;

        public TextDocumentSyncHandler(ILanguageServerFacade router, ProjectsManager projectsManager)
        {
            _router = router;
            _projectsManager = projectsManager;
        }

        public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            var filePath = request.TextDocument.Uri.GetFileSystemPath();
            if (!_projectsManager.TryGetPreviewParameters(filePath, out var parameters))
            {
                return Unit.Task;
            }

            if (_htmlPreviewerProcess is not null)
            {
                _router.SendNotification("view/avalonia/stop-preview");
                _htmlPreviewerProcess.Kill();
            }

            _htmlPreviewerProcess = CreateHtmlPreviewerProcess(parameters, HtmlUrl);
            if (_htmlPreviewerProcess is not null)
                _router.SendNotification(
                    "view/avalonia/start-preview", new StartPreviewMessage(HtmlUrl, parameters.XamlFilePath));

            return Unit.Task;
        }

        public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken) =>
            Unit.Task;

        public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken) =>
            Unit.Task;

        public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken) =>
            Unit.Task;

        public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri) =>
            new(uri, "xaml");

        protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
            SynchronizationCapability capability, ClientCapabilities clientCapabilities)
        {
            return new TextDocumentSyncRegistrationOptions
            {
                DocumentSelector = new DocumentSelector(
                    new DocumentFilter {Pattern = "**/*.axaml"},
                    new DocumentFilter {Pattern = "**/*.xaml"},
                    new DocumentFilter {Pattern = "**/*.paml"}),
                Change = TextDocumentSyncKind.Full,
                Save = new SaveOptions {IncludeText = false}
            };
        }

        private static Process? CreateHtmlPreviewerProcess(PreviewParameters parameters, string htmlUrl)
        {
            var arguments = GetHtmlPreviewArguments(parameters, htmlUrl);
            var startInfo = new ProcessStartInfo("dotnet", arguments)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
            };

            return Process.Start(startInfo);
        }

        private static string GetHtmlPreviewArguments(PreviewParameters parameters, string htmlUrl) =>
            string.Concat(
                "exec",
                $" --runtimeconfig {parameters.ProjectRuntimeConfigFilePath}",
                $" --depsfile {parameters.ProjectDepsFilePath}",
                $" {parameters.AvaloniaPreviewPath}",
                $" --transport file://{parameters.XamlFilePath}",
                $" --method html --html-url {htmlUrl}",
                $" {parameters.TargetPath}");
    }
}
