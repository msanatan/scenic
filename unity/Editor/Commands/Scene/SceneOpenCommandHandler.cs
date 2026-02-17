using System.IO;
using UnityEditor.SceneManagement;

namespace UniBridge.Editor.Commands.Scene
{
    [UniBridgeCommand("scene.open")]
    public sealed class SceneOpenCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = SceneOpenCommandParams.From(request);

            if (string.IsNullOrWhiteSpace(parameters.Path))
            {
                throw new CommandHandlingException("Missing required parameter: path");
            }

            var normalizedPath = parameters.Path.Replace('\\', '/');

            if (Path.IsPathRooted(normalizedPath))
            {
                throw new CommandHandlingException("Absolute paths are not supported. Use a project-relative path (e.g. Assets/Scenes/MyScene.unity)");
            }

            if (!File.Exists(normalizedPath))
            {
                throw new CommandHandlingException($"Scene not found: {normalizedPath}");
            }

            var openedScene = EditorSceneManager.OpenScene(normalizedPath, OpenSceneMode.Single);

            return new SceneOpenCommandResult
            {
                Scene = new SceneInfo
                {
                    Name = openedScene.name,
                    Path = openedScene.path,
                    IsDirty = openedScene.isDirty,
                },
            };
        }
    }
}
