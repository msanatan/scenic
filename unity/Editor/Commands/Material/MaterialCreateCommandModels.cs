using Newtonsoft.Json;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Material
{
    public sealed class MaterialCreateCommandParams
    {
        public string AssetPath;
        public string Shader;

        public static MaterialCreateCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException("params.assetPath is required.");
            }

            var shader = CommandModelHelpers.ReadOptionalString(payload, "shader");

            return new MaterialCreateCommandParams
            {
                AssetPath = assetPath.Trim(),
                Shader = string.IsNullOrWhiteSpace(shader) ? null : shader.Trim(),
            };
        }
    }

    public sealed class MaterialSummary
    {
        [JsonProperty("assetPath")]
        public string AssetPath;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("shader")]
        public string Shader;

        [JsonProperty("instanceId")]
        public int InstanceId;
    }

    public sealed class MaterialCreateCommandResult
    {
        [JsonProperty("material")]
        public MaterialSummary Material;
    }
}
