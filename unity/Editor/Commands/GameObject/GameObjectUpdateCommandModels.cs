using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniBridge.Editor.Commands.GameObject
{
    public sealed class GameObjectUpdateCommandParams
    {
        public string Path;
        public int? InstanceId;
        public string Name;
        public string Tag;
        public string Layer;
        public bool? IsStatic;
        public TransformInput Transform = new TransformInput();
        public bool HasTransform;

        public static GameObjectUpdateCommandParams From(CommandRequest request)
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

            var name = payload.Value<string>("name");
            if (name != null && string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name must not be empty.");
            }

            var isStaticToken = payload["isStatic"];
            var isStatic = ReadOptionalBool(isStaticToken, "params.isStatic");

            var transformToken = payload["transform"];
            var hasTransform = transformToken != null && transformToken.Type != JTokenType.Null;
            var transform = hasTransform ? ReadTransform(transformToken) : new TransformInput();

            var hasAnyUpdate =
                payload["name"] != null
                || payload["tag"] != null
                || payload["layer"] != null
                || payload["isStatic"] != null
                || hasTransform;

            if (!hasAnyUpdate)
            {
                throw new CommandHandlingException("No update fields provided.");
            }

            return new GameObjectUpdateCommandParams
            {
                Path = path,
                InstanceId = instanceId,
                Name = name == null ? null : name.Trim(),
                Tag = payload.Value<string>("tag"),
                Layer = payload.Value<string>("layer"),
                IsStatic = isStatic,
                Transform = transform,
                HasTransform = hasTransform,
            };
        }

        private static bool? ReadOptionalBool(JToken token, string label)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type == JTokenType.Boolean)
            {
                return token.Value<bool>();
            }

            throw new CommandHandlingException(label + " must be a boolean.");
        }

        private static TransformInput ReadTransform(JToken token)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException("params.transform must be an object.");
            }

            var transform = new TransformInput
            {
                Space = NormalizeSpace(obj.Value<string>("space")),
                Position = ReadVector3(obj["position"]),
                Rotation = ReadVector3(obj["rotation"]),
                Scale = ReadVector3(obj["scale"]),
            };

            if (!transform.Position.HasValue && !transform.Rotation.HasValue && !transform.Scale.HasValue)
            {
                throw new CommandHandlingException("params.transform must include at least one of: position, rotation, scale.");
            }

            return transform;
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

    public sealed class Vector3Value
    {
        [JsonProperty("x")]
        public float X;

        [JsonProperty("y")]
        public float Y;

        [JsonProperty("z")]
        public float Z;
    }

    public sealed class GameObjectTransformSnapshot
    {
        [JsonProperty("position")]
        public Vector3Value Position;

        [JsonProperty("rotation")]
        public Vector3Value Rotation;

        [JsonProperty("scale")]
        public Vector3Value Scale;
    }

    public sealed class GameObjectUpdateCommandResult
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("instanceId")]
        public int InstanceId;

        [JsonProperty("tag")]
        public string Tag;

        [JsonProperty("layer")]
        public string Layer;

        [JsonProperty("isStatic")]
        public bool IsStatic;

        [JsonProperty("transform")]
        public GameObjectTransformSnapshot Transform;
    }
}
