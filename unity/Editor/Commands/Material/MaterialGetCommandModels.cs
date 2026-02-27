using Newtonsoft.Json;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Material
{
    public sealed class MaterialGetCommandParams
    {
        public string AssetPath;

        public static MaterialGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var assetPath = CommandModelHelpers.ReadOptionalString(payload, "assetPath");
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException("params.assetPath is required.");
            }

            return new MaterialGetCommandParams
            {
                AssetPath = assetPath.Trim(),
            };
        }
    }

    public sealed class MaterialGetCommandResult
    {
        [JsonProperty("material")]
        public MaterialSummary Material;
    }
}
