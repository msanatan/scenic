using Newtonsoft.Json;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Material
{
    public sealed class MaterialAssignCommandParams
    {
        public string Path;
        public int? InstanceId;
        public string AssetPath;
        public int RendererIndex;
        public int Slot;

        public static MaterialAssignCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);
            if (string.IsNullOrWhiteSpace(selector.Path) && !selector.InstanceId.HasValue)
            {
                throw new CommandHandlingException("Provide either params.path or params.instanceId.");
            }

            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException("params.assetPath is required.");
            }

            var rendererIndex = payload.Value<int?>("rendererIndex") ?? 0;
            var slot = payload.Value<int?>("slot") ?? 0;
            if (rendererIndex < 0)
            {
                throw new CommandHandlingException("params.rendererIndex must be a non-negative integer.");
            }

            if (slot < 0)
            {
                throw new CommandHandlingException("params.slot must be a non-negative integer.");
            }

            return new MaterialAssignCommandParams
            {
                Path = selector.Path,
                InstanceId = selector.InstanceId,
                AssetPath = assetPath.Trim(),
                RendererIndex = rendererIndex,
                Slot = slot,
            };
        }
    }

    public sealed class MaterialAssignCommandResult
    {
        [JsonProperty("targetPath")]
        public string TargetPath;

        [JsonProperty("targetInstanceId")]
        public int TargetInstanceId;

        [JsonProperty("rendererType")]
        public string RendererType;

        [JsonProperty("rendererIndex")]
        public int RendererIndex;

        [JsonProperty("rendererInstanceId")]
        public int RendererInstanceId;

        [JsonProperty("slot")]
        public int Slot;

        [JsonProperty("material")]
        public MaterialSummary Material;
    }
}
