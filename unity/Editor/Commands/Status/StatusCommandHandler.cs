using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scenic.Editor.Commands.Status
{
    [ScenicCommand("status")]
    public sealed class StatusCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var projectPath = Path.GetDirectoryName(Application.dataPath) ?? string.Empty;
            var pluginVersion = PluginVersion.Get();
            var activeScene = EditorSceneManager.GetActiveScene();
            var activeSceneValue = string.IsNullOrWhiteSpace(activeScene.path) ? activeScene.name : activeScene.path;

            if (string.IsNullOrWhiteSpace(activeSceneValue))
            {
                activeSceneValue = "Untitled";
            }

            var playMode = "edit";
            if (EditorApplication.isPaused)
            {
                playMode = "paused";
            }
            else if (EditorApplication.isPlaying)
            {
                playMode = "playing";
            }

            var result = new StatusCommandResult
            {
                ProjectPath = projectPath,
                UnityVersion = Application.unityVersion,
                PluginVersion = pluginVersion,
                ActiveScene = activeSceneValue,
                PlayMode = playMode,
            };
            return result;
        }
    }
}
