using Newtonsoft.Json.Linq;

namespace UniBridge.Editor.Commands.GameObject
{
    internal static class GameObjectCommandModelHelpers
    {
        public static string NormalizeTransformSpace(string value)
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

        public static Vector3Input ReadVector3(JToken token, string label)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return new Vector3Input { HasValue = false };
            }

            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"{label} must be an object with numeric x, y, and z.");
            }

            var x = obj.Value<float?>("x");
            var y = obj.Value<float?>("y");
            var z = obj.Value<float?>("z");
            if (!x.HasValue || !y.HasValue || !z.HasValue)
            {
                throw new CommandHandlingException($"{label} must be an object with numeric x, y, and z.");
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
}
