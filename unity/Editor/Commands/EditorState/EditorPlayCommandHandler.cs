using UnityEditor;

namespace UniBridge.Editor.Commands.EditorState
{
    [UniBridgeCommand("editor.play")]
    public sealed class EditorPlayCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            EditorApplication.isPaused = false;
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }

            return new EditorStateCommandResult
            {
                PlayMode = EditorStateMode.GetCurrent(),
            };
        }
    }
}
