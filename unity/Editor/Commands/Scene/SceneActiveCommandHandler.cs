using UnityEditor.SceneManagement;

namespace UniBridge.Editor.Commands.Scene
{
    [UniBridgeCommand("scene.active")]
    public sealed class SceneActiveCommandHandler : ICommandHandler
    {
        public CommandResponse Handle(CommandRequest request)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            var pathValue = string.IsNullOrWhiteSpace(activeScene.path) ? activeScene.name : activeScene.path;
            if (string.IsNullOrWhiteSpace(pathValue))
            {
                pathValue = "Untitled";
            }

            var result = new SceneActiveCommandResult
            {
                Scene = new SceneInfo
                {
                    Name = string.IsNullOrWhiteSpace(activeScene.name) ? "Untitled" : activeScene.name,
                    Path = pathValue,
                    IsDirty = activeScene.isDirty,
                },
            };

            return CommandResponse.Ok(request == null ? string.Empty : request.Id, result);
        }
    }
}
