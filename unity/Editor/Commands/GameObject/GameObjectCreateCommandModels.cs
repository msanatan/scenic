using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor.Commands.GameObject
{
    public sealed class Vector3Input
    {
        public bool HasValue;
        public float X;
        public float Y;
        public float Z;
    }

    public sealed class TransformInput
    {
        public string Space;
        public Vector3Input Position = new Vector3Input();
        public Vector3Input Rotation = new Vector3Input();
        public Vector3Input Scale = new Vector3Input();
    }

    public sealed class GameObjectCreateCommandParams
    {
        public string Name;
        public string Parent;
        public int? ParentInstanceId;
        public string Dimension;
        public string Primitive;
        public TransformInput Transform = new TransformInput();

        public static GameObjectCreateCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);

            var name = payload.Value<string>("name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            var dimension = NormalizeDimension(payload.Value<string>("dimension"));
            var primitive = NormalizePrimitive(payload.Value<string>("primitive"));
            var space = GameObjectCommandModelHelpers.NormalizeTransformSpace(payload.SelectToken("transform.space")?.Value<string>());
            var parentSelector = CommandModelHelpers.ReadPathInstanceSelector(
                payload,
                pathParam: "parent",
                instanceIdParam: "parentInstanceId");

            var transform = new TransformInput
            {
                Space = space,
                Position = GameObjectCommandModelHelpers.ReadVector3(payload.SelectToken("transform.position"), "params.transform.position"),
                Rotation = GameObjectCommandModelHelpers.ReadVector3(payload.SelectToken("transform.rotation"), "params.transform.rotation"),
                Scale = GameObjectCommandModelHelpers.ReadVector3(payload.SelectToken("transform.scale"), "params.transform.scale"),
            };

            return new GameObjectCreateCommandParams
            {
                Name = name.Trim(),
                Parent = parentSelector.Path,
                ParentInstanceId = parentSelector.InstanceId,
                Dimension = dimension,
                Primitive = primitive,
                Transform = transform,
            };
        }

        private static string NormalizeDimension(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "3d";
            }

            var normalized = value.Trim().ToLowerInvariant();
            if (normalized == "2d" || normalized == "3d")
            {
                return normalized;
            }

            throw new CommandHandlingException("params.dimension must be one of: 2d, 3d.");
        }

        private static string NormalizePrimitive(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "cube":
                case "sphere":
                case "capsule":
                case "cylinder":
                case "plane":
                case "quad":
                    return normalized;
                default:
                    throw new CommandHandlingException("params.primitive must be one of: cube, sphere, capsule, cylinder, plane, quad.");
            }
        }

    }

    public sealed class GameObjectCreateCommandResult
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("isActive")]
        public bool IsActive;

        [JsonProperty("siblingIndex")]
        public int SiblingIndex;

        [JsonProperty("instanceId")]
        public int InstanceId;
    }
}
