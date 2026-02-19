using Newtonsoft.Json.Linq;

namespace UniBridge.Editor.Commands
{
    public sealed class PathInstanceSelector
    {
        public string Path;
        public int? InstanceId;
    }

    internal static class CommandModelHelpers
    {
        public static JObject ParsePayload(CommandRequest request)
        {
            if (request == null)
            {
                throw new CommandHandlingException("Request is required.");
            }

            try
            {
                return JObject.Parse(string.IsNullOrWhiteSpace(request.ParamsJson) ? "{}" : request.ParamsJson);
            }
            catch
            {
                throw new CommandHandlingException("Invalid params payload.");
            }
        }

        public static PathInstanceSelector ReadPathInstanceSelector(
            JObject payload,
            string pathParam = "path",
            string instanceIdParam = "instanceId")
        {
            var path = payload.Value<string>(pathParam);
            var instanceId = payload.Value<int?>(instanceIdParam);

            if (!string.IsNullOrWhiteSpace(path) && instanceId.HasValue)
            {
                throw new CommandHandlingException(
                    $"Provide either params.{pathParam} or params.{instanceIdParam}, not both.");
            }

            return new PathInstanceSelector
            {
                Path = path,
                InstanceId = instanceId,
            };
        }

        public static string ReadOptionalString(JObject payload, string paramName)
        {
            var token = payload == null ? null : payload[paramName];
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            return token.Type == JTokenType.String
                ? token.Value<string>()
                : token.ToString(Newtonsoft.Json.Formatting.None);
        }

        public static string[] ReadOptionalStringArray(JObject payload, string paramName)
        {
            var token = payload == null ? null : payload[paramName];
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (!(token is JArray values))
            {
                throw new CommandHandlingException($"params.{paramName} must be an array.");
            }

            var result = new string[values.Count];
            for (var i = 0; i < values.Count; i++)
            {
                var item = values[i];
                result[i] = item == null || item.Type == JTokenType.Null
                    ? string.Empty
                    : item.Type == JTokenType.String
                        ? item.Value<string>()
                        : item.ToString(Newtonsoft.Json.Formatting.None);
            }

            return result;
        }
    }
}
