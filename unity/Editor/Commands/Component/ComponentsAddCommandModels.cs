using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Components
{
    public sealed class ComponentsAddCommandParams
    {
        public string Path;
        public int? InstanceId;
        public string Type;
        public JObject InitialValues;
        public bool Strict;

        public static ComponentsAddCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);

            var type = payload.Value<string>("type");
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new CommandHandlingException("Missing required parameter: type");
            }

            var initialValuesToken = payload["initialValues"];
            JObject initialValues = null;
            if (initialValuesToken != null && initialValuesToken.Type != JTokenType.Null)
            {
                if (!(initialValuesToken is JObject initialValuesObj))
                {
                    throw new CommandHandlingException("params.initialValues must be a JSON object.");
                }

                initialValues = initialValuesObj;
            }

            return new ComponentsAddCommandParams
            {
                Path = selector.Path,
                InstanceId = selector.InstanceId,
                Type = type.Trim(),
                InitialValues = initialValues,
                Strict = payload.Value<bool?>("strict") ?? false,
            };
        }
    }

    public sealed class ComponentsAddCommandResult
    {
        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("appliedFields")]
        public string[] AppliedFields;

        [JsonProperty("ignoredFields")]
        public string[] IgnoredFields;
    }
}
