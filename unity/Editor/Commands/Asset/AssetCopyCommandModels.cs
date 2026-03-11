using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetCopyCommandParams
    {
        public string AssetPath;
        public string NewPath;

        public static AssetCopyCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            var newPath = CommandModelHelpers.ReadOptionalString(payload, "newPath");

            var validatedNewPath = AssetPathHelpers.RequireWritableAssetPath(newPath, "newPath");
            return new AssetCopyCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingAsset(assetPath),
                NewPath = validatedNewPath,
            };
        }
    }

    public sealed class AssetCopyCommandResult
    {
        [JsonProperty("sourcePath")]
        public string SourcePath;

        [JsonProperty("newPath")]
        public string NewPath;

        [JsonProperty("guid")]
        public string Guid;
    }
}
