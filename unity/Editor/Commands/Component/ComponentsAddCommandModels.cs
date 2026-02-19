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
            if (request == null)
            {
                throw new CommandHandlingException("Request is required.");
            }

            JObject payload;
            try
            {
                payload = JObject.Parse(string.IsNullOrWhiteSpace(request.ParamsJson) ? "{}" : request.ParamsJson);
            }
            catch
            {
                throw new CommandHandlingException("Invalid params payload.");
            }

            var path = payload.Value<string>("path");
            var instanceId = payload.Value<int?>("instanceId");
            if (!string.IsNullOrWhiteSpace(path) && instanceId.HasValue)
            {
                throw new CommandHandlingException("Provide either params.path or params.instanceId, not both.");
            }

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
                Path = path,
                InstanceId = instanceId,
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
