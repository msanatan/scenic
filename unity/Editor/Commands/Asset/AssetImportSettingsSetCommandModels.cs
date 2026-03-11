using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetImportSettingsSetCommandParams
    {
        public string AssetPath;
        public JObject Properties;

        public static AssetImportSettingsSetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            var properties = payload["properties"] as JObject;

            if (properties == null || properties.Count == 0)
            {
                throw new CommandHandlingException("params.properties is required and must be a non-empty object.");
            }

            return new AssetImportSettingsSetCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingWritableAsset(assetPath),
                Properties = properties,
            };
        }
    }

    public sealed class AssetImportSettingsSetCommandResult
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("importerType")]
        public string ImporterType;

        [JsonProperty("appliedProperties")]
        public string[] AppliedProperties;
    }
}
