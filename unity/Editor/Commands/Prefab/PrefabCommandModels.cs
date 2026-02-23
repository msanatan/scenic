using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scenic.Editor.Commands;
using Scenic.Editor.Commands.GameObject;

namespace Scenic.Editor.Commands.Prefab
{
    public sealed class PrefabInstantiateCommandParams
    {
        public string PrefabPath;
        public string ParentPath;
        public int? ParentInstanceId;
        public TransformInput Transform = new TransformInput();

        public static PrefabInstantiateCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var prefabPath = CommandModelHelpers.ReadOptionalString(payload, "prefabPath");
            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                throw new CommandHandlingException("params.prefabPath is required.");
            }

            var parentSelector = CommandModelHelpers.ReadPathInstanceSelector(
                payload,
                pathParam: "parentPath",
                instanceIdParam: "parentInstanceId");

            var transform = new TransformInput
            {
                Space = GameObjectCommandModelHelpers.NormalizeTransformSpace(payload.SelectToken("transform.space")?.Value<string>()),
                Position = GameObjectCommandModelHelpers.ReadVector3(payload.SelectToken("transform.position"), "params.transform.position"),
                Rotation = GameObjectCommandModelHelpers.ReadVector3(payload.SelectToken("transform.rotation"), "params.transform.rotation"),
                Scale = GameObjectCommandModelHelpers.ReadVector3(payload.SelectToken("transform.scale"), "params.transform.scale"),
            };

            return new PrefabInstantiateCommandParams
            {
                PrefabPath = prefabPath.Trim(),
                ParentPath = parentSelector.Path,
                ParentInstanceId = parentSelector.InstanceId,
                Transform = transform,
            };
        }
    }

    public sealed class PrefabInstantiateCommandResult
    {
        [JsonProperty("prefabPath")]
        public string PrefabPath;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("siblingIndex")]
        public int SiblingIndex;

        [JsonProperty("isActive")]
        public bool IsActive;

        [JsonProperty("transform")]
        public TransformSnapshot Transform;
    }

    public sealed class PrefabSaveCommandParams
    {
        public string PrefabPath;
        public string Path;
        public int? InstanceId;

        public static PrefabSaveCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var prefabPath = CommandModelHelpers.ReadOptionalString(payload, "prefabPath");
            if (string.IsNullOrWhiteSpace(prefabPath))
            {
                throw new CommandHandlingException("params.prefabPath is required.");
            }

            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);

            return new PrefabSaveCommandParams
            {
                PrefabPath = prefabPath.Trim(),
                Path = selector.Path,
                InstanceId = selector.InstanceId,
            };
        }
    }

    public sealed class PrefabSaveCommandResult
    {
        [JsonProperty("prefabPath")]
        public string PrefabPath;

        [JsonProperty("sourceName")]
        public string SourceName;

        [JsonProperty("sourcePath")]
        public string SourcePath;

        [JsonProperty("sourceInstanceId")]
        public int SourceInstanceId;
    }

    public sealed class TransformSnapshot
    {
        [JsonProperty("position")]
        public Vector3Output Position;

        [JsonProperty("rotation")]
        public Vector3Output Rotation;

        [JsonProperty("scale")]
        public Vector3Output Scale;
    }

    public sealed class Vector3Output
    {
        [JsonProperty("x")]
        public float X;

        [JsonProperty("y")]
        public float Y;

        [JsonProperty("z")]
        public float Z;
    }
}
