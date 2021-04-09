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
        private readonly ProjectsManager _projectsManager;
        private readonly ILanguageServerFacade _router;

        public TextDocumentSyncHandler(ILanguageServerFacade router, ProjectsManager projectsManager)
        {
            _router = router;
            _projectsManager = projectsManager;
        }

        public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            var filePath = request.TextDocument.Uri.GetFileSystemPath();
            if (_projectsManager.TryGetPreviewParameters(filePath, out var parameters))
            {
                _router.SendNotification("view/avalonia/preview", parameters!);
            }

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
            return new()
            {
                DocumentSelector = new DocumentSelector(
                    new DocumentFilter {Pattern = "**/*.axaml"},
                    new DocumentFilter {Pattern = "**/*.xaml"},
                    new DocumentFilter {Pattern = "**/*.paml"}),
                Change = TextDocumentSyncKind.Full,
                Save = new SaveOptions {IncludeText = false}
            };
        }
    }
}
