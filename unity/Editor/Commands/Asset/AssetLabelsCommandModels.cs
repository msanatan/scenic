using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetLabelsGetCommandParams
    {
        public string AssetPath;

        public static AssetLabelsGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");

            return new AssetLabelsGetCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingAsset(assetPath),
            };
        }
    }

    public sealed class AssetLabelsGetCommandResult
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("labels")]
        public string[] Labels;
    }

    public sealed class AssetLabelsAddCommandParams
    {
        public string AssetPath;
        public string[] Labels;

        public static AssetLabelsAddCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            var labels = CommandModelHelpers.ReadOptionalStringArray(payload, "labels");

            if (labels == null || labels.Length == 0)
            {
                throw new CommandHandlingException("params.labels is required and must be a non-empty array.");
            }

            return new AssetLabelsAddCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingWritableAsset(assetPath),
                Labels = labels,
            };
        }
    }

    public sealed class AssetLabelsAddCommandResult
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("labels")]
        public string[] Labels;

        [JsonProperty("added")]
        public string[] Added;
    }

    public sealed class AssetLabelsRemoveCommandParams
    {
        public string AssetPath;
        public string[] Labels;

        public static AssetLabelsRemoveCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            var labels = CommandModelHelpers.ReadOptionalStringArray(payload, "labels");

            if (labels == null || labels.Length == 0)
            {
                throw new CommandHandlingException("params.labels is required and must be a non-empty array.");
            }

            return new AssetLabelsRemoveCommandParams
            {
                AssetPath = AssetPathHelpers.RequireExistingWritableAsset(assetPath),
                Labels = labels,
            };
        }
    }

    public sealed class AssetLabelsRemoveCommandResult
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("labels")]
        public string[] Labels;

        [JsonProperty("removed")]
        public string[] Removed;
    }
}
