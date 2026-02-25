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

    public sealed class PackagesAddCommandParams
    {
        public string Name;
        public string Version;

        public static PackagesAddCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var name = CommandModelHelpers.ReadOptionalString(payload, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            var version = CommandModelHelpers.ReadOptionalString(payload, "version");

            return new PackagesAddCommandParams
            {
                Name = name.Trim(),
                Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim(),
            };
        }
    }

    public sealed class PackagesRemoveCommandParams
    {
        public string Name;

        public static PackagesRemoveCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var name = CommandModelHelpers.ReadOptionalString(payload, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            return new PackagesRemoveCommandParams
            {
                Name = name.Trim(),
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

    public sealed class PackagesAddCommandResult
    {
        [JsonProperty("package")]
        public PackageItem Package;

        [JsonProperty("added")]
        public bool Added;

        [JsonProperty("total")]
        public int Total;
    }

    public sealed class PackagesRemoveCommandResult
    {
        [JsonProperty("package")]
        public PackageItem Package;

        [JsonProperty("removed")]
        public bool Removed;

        [JsonProperty("total")]
        public int Total;
    }
}
