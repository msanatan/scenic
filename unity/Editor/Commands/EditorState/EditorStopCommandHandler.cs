using UnityEditor;

namespace Scenic.Editor.Commands.EditorState
{
    [ScenicCommand("editor.stop")]
    public sealed class EditorStopCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            EditorApplication.isPaused = false;
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
            }

            return new EditorStateCommandResult
            {
                PlayMode = EditorStateMode.GetCurrent(),
            };
        }
    }
}
