using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.SceneManagement;
using UnityEngine;
using PMPackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UniBridge.Editor.Commands.Status
{
    [UniBridgeCommand("status")]
    public sealed class StatusCommandHandler : ICommandHandler
    {
        public CommandResponse Handle(CommandRequest request)
        {
            var projectPath = Path.GetDirectoryName(Application.dataPath) ?? string.Empty;
            var pluginVersion = PMPackageInfo.FindForAssembly(typeof(UniBridgeServer).Assembly)?.version ?? "0.0.0";
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

            return CommandResponse.Ok(request == null ? string.Empty : request.Id, result.ToJson());
        }
    }
}
