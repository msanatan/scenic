using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Asset
{
    public sealed class AssetFindCommandParams
    {
        public string Query;
        public string Type;
        public string[] Labels;
        public PaginationParams Paging;

        public static AssetFindCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var query = CommandModelHelpers.ReadOptionalString(payload, "query");
            var type = CommandModelHelpers.ReadOptionalString(payload, "type");
            var labels = CommandModelHelpers.ReadOptionalStringArray(payload, "labels");

            return new AssetFindCommandParams
            {
                Query = string.IsNullOrWhiteSpace(query) ? null : query.Trim(),
                Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim(),
                Labels = labels,
                Paging = PaginationParams.From(payload, defaultLimit: 50, defaultOffset: 0),
            };
        }
    }

    public sealed class AssetFindItem
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("guid")]
        public string Guid;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("name")]
        public string Name;
    }

    public sealed class AssetFindCommandResult
    {
        [JsonProperty("assets")]
        public AssetFindItem[] Assets;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }
}
