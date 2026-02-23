using UnityEditor;

namespace Scenic.Editor.Commands.EditorState
{
    [ScenicCommand("editor.play")]
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
