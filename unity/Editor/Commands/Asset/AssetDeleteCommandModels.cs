using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetDeleteCommandParams
    {
        public string AssetPath;

        public static AssetDeleteCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");

            return new AssetDeleteCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingWritableAsset(assetPath),
            };
        }
    }

    public sealed class AssetDeleteCommandResult
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("deleted")]
        public bool Deleted;
    }
}
