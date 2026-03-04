using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Material
{
    public sealed class MaterialPropertiesGetCommandParams
    {
        public string AssetPath;
        public string[] Names;

        public static MaterialPropertiesGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException("params.assetPath is required.");
            }

            var names = CommandModelHelpers.ReadOptionalStringArray(payload, "names");
            return new MaterialPropertiesGetCommandParams
            {
                AssetPath = assetPath.Trim(),
                Names = names,
            };
        }
    }

    public sealed class MaterialPropertiesSetCommandParams
    {
        public string AssetPath;
        public JObject Values;
        public bool Strict;

        public static MaterialPropertiesSetCommandParams From(CommandRequest request)
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
                throw new CommandHandlingException("params.values must include at least one property.");
            }

            return new MaterialPropertiesSetCommandParams
            {
                AssetPath = assetPath.Trim(),
                Values = valuesObj,
                Strict = payload.Value<bool?>("strict") ?? false,
            };
        }
    }

    public sealed class MaterialPropertyValue
    {
        [JsonProperty("type")]
        public string Type;

        [JsonProperty("value")]
        public object Value;

        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("textureType")]
        public string TextureType;
    }

    public sealed class MaterialPropertiesGetCommandResult
    {
        [JsonProperty("material")]
        public MaterialSummary Material;

        [JsonProperty("properties")]
        public JObject Properties;
    }

    public sealed class MaterialPropertiesSetCommandResult
    {
        [JsonProperty("material")]
        public MaterialSummary Material;

        [JsonProperty("appliedProperties")]
        public string[] AppliedProperties;

        [JsonProperty("ignoredProperties")]
        public string[] IgnoredProperties;
    }
}
