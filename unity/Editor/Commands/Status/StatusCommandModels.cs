namespace UniBridge.Editor.Commands.Status
{
    public sealed class StatusCommandResult
    {
        public string ProjectPath;
        public string UnityVersion;
        public string PluginVersion;
        public string ActiveScene;
        public string PlayMode;

        public string ToJson()
        {
            return "{" +
                   "\"projectPath\":" + JsonCompat.Quote(ProjectPath) + "," +
                   "\"unityVersion\":" + JsonCompat.Quote(UnityVersion) + "," +
                   "\"pluginVersion\":" + JsonCompat.Quote(PluginVersion) + "," +
                   "\"activeScene\":" + JsonCompat.Quote(ActiveScene) + "," +
                   "\"playMode\":" + JsonCompat.Quote(PlayMode) +
                   "}";
        }
    }
}
