using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetGetCommandParams
    {
        public string AssetPath;

        public static AssetGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");

            return new AssetGetCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingAsset(assetPath),
            };
        }
    }

    public sealed class AssetGetCommandResult
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("guid")]
        public string Guid;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("labels")]
        public string[] Labels;

        [JsonProperty("dependencies")]
        public string[] Dependencies;
    }
}
