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
    }
}
