using Newtonsoft.Json;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Components
{
    public sealed class ComponentsListCommandParams
    {
        public string Path;
        public int? InstanceId;
        public string Type;
        public PaginationParams Paging;

        private const int DefaultLimit = 50;
        private const int DefaultOffset = 0;

        public static ComponentsListCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);
            var type = CommandModelHelpers.ReadOptionalString(payload, "type");

            return new ComponentsListCommandParams
            {
                Path = selector.Path,
                InstanceId = selector.InstanceId,
                Type = string.IsNullOrWhiteSpace(type) ? null : type,
                Paging = PaginationParams.From(payload, defaultLimit: DefaultLimit, defaultOffset: DefaultOffset),
            };
        }
    }

    public sealed class ComponentListItem
    {
        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("index")]
        public int Index;

        [JsonProperty("enabled")]
        public bool? Enabled;
    }

    public sealed class ComponentsListCommandResult
    {
        [JsonProperty("components")]
        public ComponentListItem[] Components;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }
}
