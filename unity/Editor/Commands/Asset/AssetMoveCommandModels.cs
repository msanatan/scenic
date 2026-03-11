using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetMoveCommandParams
    {
        public string AssetPath;
        public string NewPath;

        public static AssetMoveCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            var newPath = CommandModelHelpers.ReadOptionalString(payload, "newPath");

            var validatedNewPath = AssetPathHelpers.RequireWritableAssetPath(newPath, "newPath");
            return new AssetMoveCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingAsset(assetPath),
                NewPath = validatedNewPath,
            };
        }
    }

    public sealed class AssetMoveCommandResult
    {
        [JsonProperty("oldPath")]
        public string OldPath;

        [JsonProperty("newPath")]
        public string NewPath;

        [JsonProperty("guid")]
        public string Guid;
    }
}
