using UnityEditor.SceneManagement;

namespace Scenic.Editor.Commands.Scene
{
    [ScenicCommand("scene.active")]
    public sealed class SceneActiveCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
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
            return result;
        }
    }
}
