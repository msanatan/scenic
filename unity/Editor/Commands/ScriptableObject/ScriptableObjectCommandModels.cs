using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.ScriptableObjects
{
    public sealed class ScriptableObjectCreateCommandParams
    {
        public string AssetPath;
        public string Type;
        public JObject InitialValues;
        public bool Strict;

        public static ScriptableObjectCreateCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);

            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException("params.assetPath is required.");
            }

            var type = CommandModelHelpers.ReadOptionalString(payload, "type");
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new CommandHandlingException("params.type is required.");
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

            return new ScriptableObjectCreateCommandParams
            {
                AssetPath = assetPath.Trim(),
                Type = type.Trim(),
                InitialValues = initialValues,
                Strict = payload.Value<bool?>("strict") ?? false,
            };
        }
    }

    public sealed class ScriptableObjectGetCommandParams
    {
        public string AssetPath;

        public static ScriptableObjectGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException("params.assetPath is required.");
            }

            return new ScriptableObjectGetCommandParams
            {
                AssetPath = assetPath.Trim(),
            };
        }
    }

    public sealed class ScriptableObjectUpdateCommandParams
    {
        public string AssetPath;
        public JObject Values;
        public bool Strict;

        public static ScriptableObjectUpdateCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException("params.assetPath is required.");
            }

            var valuesToken = payload["values"];
            if (!(valuesToken is JObject valuesObj))
            {
                throw new CommandHandlingException("params.values must be a JSON object.");
            }

            if (valuesObj.Count == 0)
            {
                throw new CommandHandlingException("params.values must include at least one field.");
            }

            return new ScriptableObjectUpdateCommandParams
            {
                AssetPath = assetPath.Trim(),
                Values = valuesObj,
                Strict = payload.Value<bool?>("strict") ?? false,
            };
        }
    }

    public sealed class ScriptableObjectSummary
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("type")]
        public string Type;

        [JsonProperty("instanceId")]
        public int InstanceId;
    }

    public sealed class ScriptableObjectCreateCommandResult
    {
        [JsonProperty("asset")]
        public ScriptableObjectSummary Asset;

        [JsonProperty("appliedFields")]
        public string[] AppliedFields;

        [JsonProperty("ignoredFields")]
        public string[] IgnoredFields;
    }

    public sealed class ScriptableObjectGetCommandResult
    {
        [JsonProperty("asset")]
        public ScriptableObjectSummary Asset;

        [JsonProperty("serialized")]
        public object Serialized;
    }

    public sealed class ScriptableObjectUpdateCommandResult
    {
        [JsonProperty("asset")]
        public ScriptableObjectSummary Asset;

        [JsonProperty("appliedFields")]
        public string[] AppliedFields;

        [JsonProperty("ignoredFields")]
        public string[] IgnoredFields;
    }
}
