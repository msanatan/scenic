using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Scenic.Editor.Commands.Scene
{
    [ScenicCommand("scene.create")]
    public sealed class SceneCreateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = SceneCreateCommandParams.From(request);

            if (string.IsNullOrWhiteSpace(parameters.Path))
            {
                throw new CommandHandlingException("Missing required parameter: path");
            }

            var normalizedPath = parameters.Path.Replace('\\', '/');

            if (Path.IsPathRooted(normalizedPath))
            {
                throw new CommandHandlingException("Absolute paths are not supported. Use a project-relative path (e.g. Assets/Scenes/MyScene.unity)");
            }

            if (File.Exists(normalizedPath))
            {
                throw new CommandHandlingException($"Scene already exists: {normalizedPath}");
            }

            var parentDir = Path.GetDirectoryName(normalizedPath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
            {
                throw new CommandHandlingException($"Directory does not exist: {parentDir}");
            }

            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, normalizedPath);

            return new SceneCreateCommandResult
            {
                Scene = new SceneInfo
                {
                    Name = newScene.name,
                    Path = newScene.path,
                    IsDirty = newScene.isDirty,
                },
            };
        }
    }
}
