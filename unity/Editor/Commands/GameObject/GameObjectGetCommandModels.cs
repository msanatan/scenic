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

            return new GameObjectGetCommandParams
            {
                Path = path,
                InstanceId = instanceId,
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
