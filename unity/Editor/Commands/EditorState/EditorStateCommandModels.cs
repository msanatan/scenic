using Newtonsoft.Json;
using UnityEditor;

namespace UniBridge.Editor.Commands.EditorState
{
    internal static class EditorStateMode
    {
        public static string GetCurrent()
        {
            if (EditorApplication.isPaused)
            {
                return "paused";
            }

            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return "playing";
            }

            return "edit";
        }
    }

    public sealed class EditorStateCommandResult
    {
        [JsonProperty("playMode")]
        public string PlayMode;
    }
}
