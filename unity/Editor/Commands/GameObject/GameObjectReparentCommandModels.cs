using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor.Commands.GameObject
{
    public sealed class GameObjectReparentCommandParams
    {
        public string Path;
        public int? InstanceId;
        public string ParentPath;
        public int? ParentInstanceId;
        public bool ToRoot;
        public bool WorldPositionStays;

        public static GameObjectReparentCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var targetSelector = CommandModelHelpers.ReadPathInstanceSelector(payload);
            var parentSelector = CommandModelHelpers.ReadPathInstanceSelector(
                payload,
                pathParam: "parentPath",
                instanceIdParam: "parentInstanceId");

            var toRoot = payload.Value<bool?>("toRoot") ?? false;
            if (toRoot && (!string.IsNullOrWhiteSpace(parentSelector.Path) || parentSelector.InstanceId.HasValue))
            {
                throw new CommandHandlingException("params.toRoot cannot be combined with parentPath or parentInstanceId.");
            }

            if (!toRoot && string.IsNullOrWhiteSpace(parentSelector.Path) && !parentSelector.InstanceId.HasValue)
            {
                throw new CommandHandlingException("Provide destination parent via parentPath/parentInstanceId, or set toRoot=true.");
            }

            return new GameObjectReparentCommandParams
            {
                Path = targetSelector.Path,
                InstanceId = targetSelector.InstanceId,
                ParentPath = parentSelector.Path,
                ParentInstanceId = parentSelector.InstanceId,
                ToRoot = toRoot,
                WorldPositionStays = payload.Value<bool?>("worldPositionStays") ?? false,
            };
        }
    }

    public sealed class GameObjectReparentCommandResult
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("parentPath")]
        public string ParentPath;

        [JsonProperty("siblingIndex")]
        public int SiblingIndex;
    }
}
