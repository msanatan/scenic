using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetImportSettingsGetCommandParams
    {
        public string AssetPath;
        public string[] Properties;

        public static AssetImportSettingsGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            var properties = CommandModelHelpers.ReadOptionalStringArray(payload, "properties");

            return new AssetImportSettingsGetCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingAsset(assetPath),
                Properties = properties,
            };
        }
    }

    public sealed class AssetImportSettingsGetCommandResult
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("importerType")]
        public string ImporterType;

        [JsonProperty("properties")]
        public JObject Properties;
    }
}
