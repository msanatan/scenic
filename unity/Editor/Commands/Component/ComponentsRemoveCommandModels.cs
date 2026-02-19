using Newtonsoft.Json;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Components
{
    public sealed class ComponentsRemoveCommandParams
    {
        public string Path;
        public int? InstanceId;
        public int? ComponentInstanceId;
        public int? Index;
        public string Type;

        public static ComponentsRemoveCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);
            var type = CommandModelHelpers.ReadOptionalString(payload, "type");

            var componentInstanceId = payload.Value<int?>("componentInstanceId");
            var index = payload.Value<int?>("index");
            var hasType = !string.IsNullOrWhiteSpace(type);
            var selectors = (componentInstanceId.HasValue ? 1 : 0) + (index.HasValue ? 1 : 0) + (hasType ? 1 : 0);
            if (selectors != 1)
            {
                throw new CommandHandlingException("Provide exactly one selector: params.componentInstanceId, params.index, or params.type.");
            }

            if (index.HasValue && index.Value < 0)
            {
                throw new CommandHandlingException("params.index must be a non-negative integer.");
            }

            return new ComponentsRemoveCommandParams
            {
                Path = selector.Path,
                InstanceId = selector.InstanceId,
                ComponentInstanceId = componentInstanceId,
                Index = index,
                Type = hasType ? type.Trim() : null,
            };
        }
    }

    public sealed class ComponentsRemoveCommandResult
    {
        [JsonProperty("removed")]
        public bool Removed;

        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("index")]
        public int Index;
    }
}
