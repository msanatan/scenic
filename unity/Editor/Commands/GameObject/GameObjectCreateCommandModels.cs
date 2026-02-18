using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniBridge.Editor.Commands.GameObject
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

            var name = payload.Value<string>("name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("Missing required parameter: name");
            }

            var dimension = NormalizeDimension(payload.Value<string>("dimension"));
            var primitive = NormalizePrimitive(payload.Value<string>("primitive"));
            var space = NormalizeSpace(payload.SelectToken("transform.space")?.Value<string>());
            var parentPath = payload.Value<string>("parent");
            var parentInstanceId = payload.Value<int?>("parentInstanceId");
            if (!string.IsNullOrWhiteSpace(parentPath) && parentInstanceId.HasValue)
            {
                throw new CommandHandlingException("Provide either params.parent or params.parentInstanceId, not both.");
            }

            var transform = new TransformInput
            {
                Space = space,
                Position = ReadVector3(payload.SelectToken("transform.position")),
                Rotation = ReadVector3(payload.SelectToken("transform.rotation")),
                Scale = ReadVector3(payload.SelectToken("transform.scale")),
            };

            return new GameObjectCreateCommandParams
            {
                Name = name.Trim(),
                Parent = parentPath,
                ParentInstanceId = parentInstanceId,
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

        private static string NormalizeSpace(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "local";
            }

            var normalized = value.Trim().ToLowerInvariant();
            if (normalized == "local" || normalized == "world")
            {
                return normalized;
            }

            throw new CommandHandlingException("params.transform.space must be one of: local, world.");
        }

        private static Vector3Input ReadVector3(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return new Vector3Input { HasValue = false };
            }

            if (!(token is JObject obj))
            {
                throw new CommandHandlingException("Vector3 values must be objects with x,y,z.");
            }

            var x = obj.Value<float?>("x");
            var y = obj.Value<float?>("y");
            var z = obj.Value<float?>("z");
            if (!x.HasValue || !y.HasValue || !z.HasValue)
            {
                throw new CommandHandlingException("Vector3 values must include numeric x,y,z.");
            }

            return new Vector3Input
            {
                HasValue = true,
                X = x.Value,
                Y = y.Value,
                Z = z.Value,
            };
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
