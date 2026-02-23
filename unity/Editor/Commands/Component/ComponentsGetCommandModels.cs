using Newtonsoft.Json;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Components
{
    public sealed class ComponentsGetCommandParams
    {
        public string Path;
        public int? InstanceId;
        public int? ComponentInstanceId;
        public int? Index;
        public string Type;

        public static ComponentsGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);
            var type = CommandModelHelpers.ReadOptionalString(payload, "type");

            var componentInstanceId = payload.Value<int?>("componentInstanceId");
            var index = payload.Value<int?>("index");
            ComponentCommandParamsHelpers.ValidateSelector(componentInstanceId, index, type);
            var hasType = !string.IsNullOrWhiteSpace(type);

            return new ComponentsGetCommandParams
            {
                Path = selector.Path,
                InstanceId = selector.InstanceId,
                ComponentInstanceId = componentInstanceId,
                Index = index,
                Type = hasType ? type.Trim() : null,
            };
        }
    }

    public sealed class ComponentGetItem
    {
        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("index")]
        public int Index;

        [JsonProperty("enabled")]
        public bool? Enabled;

        [JsonProperty("serialized")]
        public object Serialized;
    }

    public sealed class ComponentsGetCommandResult
    {
        [JsonProperty("component")]
        public ComponentGetItem Component;
    }
}
