using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniBridge.Editor.Commands.GameObject
{
    public sealed class GameObjectDestroySelector
    {
        public string Path;
        public int? InstanceId;
    }

    public sealed class GameObjectDestroyCommandParams
    {
        public GameObjectDestroySelector Target = new GameObjectDestroySelector();

        public static GameObjectDestroyCommandParams From(CommandRequest request)
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

            return new GameObjectDestroyCommandParams
            {
                Target = new GameObjectDestroySelector
                {
                    Path = payload.Value<string>("path"),
                    InstanceId = payload.Value<int?>("instanceId"),
                },
            };
        }
    }

    public sealed class GameObjectDestroyCommandResult
    {
        [JsonProperty("destroyed")]
        public bool Destroyed;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("instanceId")]
        public int InstanceId;
    }
}
