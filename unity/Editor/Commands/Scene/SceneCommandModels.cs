using Newtonsoft.Json;

namespace UniBridge.Editor.Commands.Scene
{
    public sealed class SceneInfo
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("path")]
        public string Path;

        [JsonProperty("isDirty")]
        public bool IsDirty;
    }

    public sealed class SceneActiveCommandResult
    {
        [JsonProperty("scene")]
        public SceneInfo Scene;
    }

    public sealed class SceneOpenCommandParams
    {
        public string Path;

        public static SceneOpenCommandParams From(CommandRequest request)
        {
            return new SceneOpenCommandParams
            {
                Path = request == null ? null : request.GetStringParam("path"),
            };
        }
    }

    public sealed class SceneOpenCommandResult
    {
        [JsonProperty("scene")]
        public SceneInfo Scene;
    }

    public sealed class SceneCreateCommandParams
    {
        public string Path;

        public static SceneCreateCommandParams From(CommandRequest request)
        {
            return new SceneCreateCommandParams
            {
                Path = request == null ? null : request.GetStringParam("path"),
            };
        }
    }

    public sealed class SceneCreateCommandResult
    {
        [JsonProperty("scene")]
        public SceneInfo Scene;
    }
}
