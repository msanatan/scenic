using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor.Commands.GameObject
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
            var payload = CommandModelHelpers.ParsePayload(request);
            var selector = CommandModelHelpers.ReadPathInstanceSelector(payload);

            return new GameObjectDestroyCommandParams
            {
                Target = new GameObjectDestroySelector
                {
                    Path = selector.Path,
                    InstanceId = selector.InstanceId,
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
