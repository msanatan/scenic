using Newtonsoft.Json;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Components
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
            var path = request == null ? null : request.GetStringParam("path");
            var instanceIdText = request == null ? null : request.GetStringParam("instanceId");
            var instanceId = ParseOptionalInstanceId(instanceIdText);
            if (!string.IsNullOrWhiteSpace(path) && instanceId.HasValue)
            {
                throw new CommandHandlingException("Provide either params.path or params.instanceId, not both.");
            }

            var type = request == null ? null : request.GetStringParam("type");

            return new ComponentsListCommandParams
            {
                Path = path,
                InstanceId = instanceId,
                Type = string.IsNullOrWhiteSpace(type) ? null : type,
                Paging = PaginationParams.From(request, defaultLimit: DefaultLimit, defaultOffset: DefaultOffset),
            };
        }

        private static int? ParseOptionalInstanceId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            int parsed;
            if (!int.TryParse(value, out parsed))
            {
                throw new CommandHandlingException("params.instanceId must be an integer.");
            }

            return parsed;
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
