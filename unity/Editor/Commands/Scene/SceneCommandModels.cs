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
}
