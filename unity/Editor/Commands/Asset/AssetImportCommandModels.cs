using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetImportCommandParams
    {
        public string AssetPath;
        public string Options;

        public static AssetImportCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            var options = CommandModelHelpers.ReadOptionalString(payload, "options");

            return new AssetImportCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingAsset(assetPath),
                Options = string.IsNullOrWhiteSpace(options) ? null : options.Trim(),
            };
        }
    }

    public sealed class AssetImportCommandResult
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("importerType")]
        public string ImporterType;
    }
}
