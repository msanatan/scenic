using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Status
{
    public sealed class StatusCommandResult
    {
        [JsonProperty("projectPath")]
        public string ProjectPath;

        [JsonProperty("unityVersion")]
        public string UnityVersion;

        [JsonProperty("pluginVersion")]
        public string PluginVersion;

        [JsonProperty("activeScene")]
        public string ActiveScene;

        [JsonProperty("playMode")]
        public string PlayMode;
    }
}
