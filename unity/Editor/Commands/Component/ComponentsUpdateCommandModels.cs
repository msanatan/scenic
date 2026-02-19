using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Components
{
    public sealed class ComponentsUpdateCommandParams
    {
        public string Path;
        public int? InstanceId;
        public int? ComponentInstanceId;
        public int? Index;
        public string Type;
        public JObject Values;
        public bool Strict;

        public static ComponentsUpdateCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);
            var type = CommandModelHelpers.ReadOptionalString(payload, "type");

            var componentInstanceId = payload.Value<int?>("componentInstanceId");
            var index = payload.Value<int?>("index");
            ComponentCommandParamsHelpers.ValidateSelector(componentInstanceId, index, type);

            var valuesToken = payload["values"];
            if (!(valuesToken is JObject values))
            {
                throw new CommandHandlingException("params.values must be a JSON object.");
            }

            return new ComponentsUpdateCommandParams
            {
                Path = selector.Path,
                InstanceId = selector.InstanceId,
                ComponentInstanceId = componentInstanceId,
                Index = index,
                Type = string.IsNullOrWhiteSpace(type) ? null : type.Trim(),
                Values = values,
                Strict = payload.Value<bool?>("strict") ?? false,
            };
        }
    }

    public sealed class ComponentsUpdateCommandResult
    {
        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("index")]
        public int Index;

        [JsonProperty("appliedFields")]
        public string[] AppliedFields;

        [JsonProperty("ignoredFields")]
        public string[] IgnoredFields;
    }
}
