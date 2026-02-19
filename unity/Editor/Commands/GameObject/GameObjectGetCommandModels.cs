using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniBridge.Editor.Commands.GameObject
{
    public sealed class GameObjectGetCommandParams
    {
        public string Path;
        public int? InstanceId;

        public static GameObjectGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);

            return new GameObjectGetCommandParams
            {
                Path = selector.Path,
                InstanceId = selector.InstanceId,
            };
        }
    }

    public sealed class GameObjectGetCommandResult
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("isActive")]
        public bool IsActive;

        [JsonProperty("tag")]
        public string Tag;

        [JsonProperty("layer")]
        public string Layer;

        [JsonProperty("isStatic")]
        public bool IsStatic;

        [JsonProperty("parentPath")]
        public string ParentPath;

        [JsonProperty("siblingIndex")]
        public int SiblingIndex;

        [JsonProperty("transform")]
        public GameObjectTransformSnapshot Transform;
    }
}
