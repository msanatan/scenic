using UnityEditor;

namespace Scenic.Editor.Commands.EditorState
{
    [ScenicCommand("editor.pause")]
    public sealed class EditorPauseCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new CommandHandlingException("Cannot pause while Unity is not in play mode.");
            }

            EditorApplication.isPaused = true;
            return new EditorStateCommandResult
            {
                PlayMode = EditorStateMode.GetCurrent(),
            };
        }
    }
}
