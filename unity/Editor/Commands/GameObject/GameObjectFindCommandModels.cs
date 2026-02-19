using Newtonsoft.Json;

namespace UniBridge.Editor.Commands.GameObject
{
    public sealed class GameObjectFindCommandParams
    {
        public string Query;
        public string ScenePath;
        public bool IncludeInactive;
        public PaginationParams Paging;

        private const int DefaultLimit = 50;
        private const int DefaultOffset = 0;

        public static GameObjectFindCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var query = CommandModelHelpers.ReadOptionalString(payload, "query");
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new CommandHandlingException("params.query is required.");
            }

            var scenePath = CommandModelHelpers.ReadOptionalString(payload, "scenePath");
            var includeInactive = payload.Value<bool?>("includeInactive") ?? false;

            return new GameObjectFindCommandParams
            {
                Query = query.Trim(),
                ScenePath = string.IsNullOrWhiteSpace(scenePath) ? null : scenePath.Trim(),
                IncludeInactive = includeInactive,
                Paging = PaginationParams.From(payload, defaultLimit: DefaultLimit, defaultOffset: DefaultOffset),
            };
        }
    }

    public sealed class GameObjectFindItem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("isActive")]
        public bool IsActive;

        [JsonProperty("parentPath")]
        public string ParentPath;

        [JsonProperty("siblingIndex")]
        public int SiblingIndex;
    }

    public sealed class GameObjectFindCommandResult
    {
        [JsonProperty("gameObjects")]
        public GameObjectFindItem[] GameObjects;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }
}
