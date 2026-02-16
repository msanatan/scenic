using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniBridge.Editor
{
    [InitializeOnLoad]
    public static class UniBridgeServer
    {
        private static PipeServer _server;
        private static UniBridgeSettings _settings;
        private static string _projectHash;

        static UniBridgeServer()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterReload;
            EditorApplication.quitting += OnQuit;

            Start();
        }

        private static void Start()
        {
            var projectPath = Path.GetDirectoryName(Application.dataPath);
            _projectHash = StateManager.ProjectHash(projectPath);
            _settings = UniBridgeSettings.LoadOrDefault();

            StateManager.SetCurrentProjectHash(_projectHash);
            StateManager.EnsureStateDirectory(_projectHash);
            StateManager.WriteServerJson(
                _projectHash,
                projectPath,
                protocolVersion: 1,
                executeEnabled: _settings.ExecuteEnabled);

            _server = new PipeServer(_projectHash);
            _server.OnCommandReceived += OnCommand;
            _server.Start();
            UnityEngine.Debug.Log($"UniBridgeServer started for project '{projectPath}' with hash '{_projectHash}'.");
        }

        private static void OnCommand(CommandRequest request)
        {
            UnityEngine.Debug.Log($"Received command: {request.Command} with id: {request.Id}");
            EditorApplication.delayCall += () =>
            {
                if (!_settings.ExecuteEnabled && string.Equals(request.Command, "execute", StringComparison.OrdinalIgnoreCase))
                {
                    var disabled = CommandResponse.Fail(request.Id, "Execute is disabled by plugin configuration.");
                    StateManager.WriteResultForCurrentProject(request.Id, disabled);
                    _server.Send(disabled);
                    return;
                }

                var response = CommandRouter.Route(request);
                StateManager.WriteResultForCurrentProject(request.Id, response);
                _server.Send(response);
            };
        }

        private static void OnBeforeReload()
        {
            _server?.Stop();
        }

        private static void OnAfterReload()
        {
            Start();
        }

        private static void OnQuit()
        {
            _server?.Stop();
            StateManager.Cleanup();
        }
    }

    public sealed class UniBridgeSettings
    {
        public bool ExecuteEnabled = true;

        public static UniBridgeSettings LoadOrDefault()
        {
            return new UniBridgeSettings();
        }
    }
}
