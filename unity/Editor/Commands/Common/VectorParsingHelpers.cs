using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Scenic.Editor.Commands.Common
{
    internal static class VectorParsingHelpers
    {
        public static Vector2 ParseVector2(JToken token, string fieldName, string labelPrefix)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"{labelPrefix}.{fieldName} must be an object with x,y.");
            }

            return new Vector2(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.y is required."));
        }

        public static Vector3 ParseVector3(JToken token, string fieldName, string labelPrefix)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"{labelPrefix}.{fieldName} must be an object with x,y,z.");
            }

            return new Vector3(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.y is required."),
                obj.Value<float?>("z") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.z is required."));
        }

        public static Vector4 ParseVector4(JToken token, string fieldName, string labelPrefix)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"{labelPrefix}.{fieldName} must be an object with x,y,z,w.");
            }

            return new Vector4(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.y is required."),
                obj.Value<float?>("z") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.z is required."),
                obj.Value<float?>("w") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.w is required."));
        }
    }
}
