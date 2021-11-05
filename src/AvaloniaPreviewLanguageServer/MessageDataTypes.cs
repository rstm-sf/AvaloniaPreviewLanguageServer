using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AvaloniaPreviewLanguageServer
{
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    public record StartPreviewMessage(string HtmlUrl, string XamlFilePath);
}
