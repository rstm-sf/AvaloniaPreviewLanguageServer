using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AvaloniaPreviewLanguageServer
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record PreviewParameters(
        string AvaloniaPreviewPath,
        string TargetPath,
        string ProjectDepsFilePath,
        string ProjectRuntimeConfigFilePath,
        string XamlFilePath = "");
}
