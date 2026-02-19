using Newtonsoft.Json;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Scene
{
    public sealed class SceneListItem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;
    }

    public sealed class SceneListCommandParams
    {
        public string Filter;
        public PaginationParams Paging;

        public static SceneListCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var filter = payload.Value<string>("filter");

            return new SceneListCommandParams
            {
                Filter = string.IsNullOrWhiteSpace(filter) ? null : filter,
                Paging = PaginationParams.From(request, defaultLimit: 50, defaultOffset: 0),
            };
        }
    }

    public sealed class SceneListCommandResult
    {
        [JsonProperty("scenes")]
        public SceneListItem[] Scenes;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }

    public sealed class SceneHierarchyCommandParams
    {
        public PaginationParams Paging;

        public static SceneHierarchyCommandParams From(CommandRequest request)
        {
            CommandModelHelpers.ParsePayload(request);
            return new SceneHierarchyCommandParams
            {
                Paging = PaginationParams.From(request, defaultLimit: 200, defaultOffset: 0),
            };
        }
    }

    public sealed class SceneHierarchyNode
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("isActive")]
        public bool IsActive;

        [JsonProperty("depth")]
        public int Depth;

        [JsonProperty("parentIndex")]
        public int ParentIndex;

        [JsonProperty("siblingIndex")]
        public int SiblingIndex;

        [JsonProperty("instanceId")]
        public int InstanceId;
    }

    public sealed class SceneHierarchyCommandResult
    {
        [JsonProperty("nodes")]
        public SceneHierarchyNode[] Nodes;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }

    public sealed class SceneInfo
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("isDirty")]
        public bool IsDirty;
    }

    public sealed class SceneActiveCommandResult
    {
        [JsonProperty("scene")]
        public SceneInfo Scene;
    }

    public sealed class SceneOpenCommandParams
    {
        public string Path;

        public static SceneOpenCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            return new SceneOpenCommandParams
            {
                Path = payload.Value<string>("path"),
            };
        }
    }

    public sealed class SceneOpenCommandResult
    {
        [JsonProperty("scene")]
        public SceneInfo Scene;
    }

    public sealed class SceneCreateCommandParams
    {
        public string Path;

        public static SceneCreateCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            return new SceneCreateCommandParams
            {
                Path = payload.Value<string>("path"),
            };
        }
    }

    public sealed class SceneCreateCommandResult
    {
        [JsonProperty("scene")]
        public SceneInfo Scene;
    }
}
