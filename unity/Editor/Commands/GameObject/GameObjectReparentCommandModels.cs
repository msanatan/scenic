using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniBridge.Editor.Commands.GameObject
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

            var parentPath = payload.Value<string>("parentPath");
            var parentInstanceId = payload.Value<int?>("parentInstanceId");
            if (!string.IsNullOrWhiteSpace(parentPath) && parentInstanceId.HasValue)
            {
                throw new CommandHandlingException("Provide either params.parentPath or params.parentInstanceId, not both.");
            }

            var toRoot = payload.Value<bool?>("toRoot") ?? false;
            if (toRoot && (!string.IsNullOrWhiteSpace(parentPath) || parentInstanceId.HasValue))
            {
                throw new CommandHandlingException("params.toRoot cannot be combined with parentPath or parentInstanceId.");
            }

            if (!toRoot && string.IsNullOrWhiteSpace(parentPath) && !parentInstanceId.HasValue)
            {
                throw new CommandHandlingException("Provide destination parent via parentPath/parentInstanceId, or set toRoot=true.");
            }

            return new GameObjectReparentCommandParams
            {
                Path = path,
                InstanceId = instanceId,
                ParentPath = parentPath,
                ParentInstanceId = parentInstanceId,
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
