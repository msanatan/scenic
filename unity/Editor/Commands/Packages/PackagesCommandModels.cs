using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Packages
{
    public sealed class PackagesGetCommandParams
    {
        public PaginationParams Paging;
        public bool IncludeIndirect;
        public string Search;

        public static PackagesGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var search = CommandModelHelpers.ReadOptionalString(payload, "search");

            return new PackagesGetCommandParams
            {
                Paging = PaginationParams.From(payload, defaultLimit: 50, defaultOffset: 0),
                IncludeIndirect = payload.Value<bool?>("includeIndirect") ?? false,
                Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            };
        }
    }

    public sealed class PackageItem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("displayName")]
        public string DisplayName;

        [JsonProperty("version")]
        public string Version;

        [JsonProperty("source")]
        public string Source;

        [JsonProperty("isDirectDependency")]
        public bool IsDirectDependency;
    }

    public sealed class PackagesGetCommandResult
    {
        [JsonProperty("packages")]
        public PackageItem[] Packages;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }
}
