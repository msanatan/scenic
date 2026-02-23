using Newtonsoft.Json;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Components
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
            ComponentCommandParamsHelpers.ValidateSelector(componentInstanceId, index, type);
            var hasType = !string.IsNullOrWhiteSpace(type);

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
