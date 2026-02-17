using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PMPackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace UniBridge.Editor
{
    [InitializeOnLoad]
    public static class UniBridgeServer
    {
        private static PipeServer _server;
        private static UniBridgeSettings _settings;
        private static string _projectHash;
        private static readonly ConcurrentQueue<CommandRequest> _commandQueue = new ConcurrentQueue<CommandRequest>();

        static UniBridgeServer()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterReload;
            EditorApplication.quitting += OnQuit;
            EditorApplication.update += ProcessCommandQueue;

            Start();
        }

        private static void Start()
        {
            var projectPath = Path.GetDirectoryName(Application.dataPath);
            _projectHash = StateManager.ProjectHash(projectPath);
            _settings = UniBridgeSettings.LoadOrDefault();

            StateManager.SetCurrentProjectHash(_projectHash);
            StateManager.EnsureStateDirectory(_projectHash);
            var pluginVersion = PMPackageInfo.FindForAssembly(typeof(UniBridgeServer).Assembly)?.version ?? "0.0.0";
            StateManager.WriteServerJson(
                _projectHash,
                projectPath,
                protocolVersion: 1,
                executeEnabled: _settings.ExecuteEnabled,
                pluginVersion: pluginVersion);

            _server = new PipeServer(_projectHash);
            _server.OnCommandReceived += OnCommand;
            _server.Start();
            RequeuePendingRequests();
            UnityEngine.Debug.Log($"UniBridgeServer started for project '{projectPath}' with hash '{_projectHash}'.");
        }

        private static void OnCommand(CommandRequest request)
        {
            UnityEngine.Debug.Log($"Received command: {request.Command} with id: {request.Id}");
            EnqueueCommand(request, persistRequest: true);
        }

        private static void RequeuePendingRequests()
        {
            var pending = StateManager.ListPendingRequestsForCurrentProject();
            for (var i = 0; i < pending.Length; i++)
            {
                EnqueueCommand(pending[i], persistRequest: false);
            }
        }

        private static void EnqueueCommand(CommandRequest request, bool persistRequest)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Id))
            {
                return;
            }

            if (persistRequest)
            {
                StateManager.WriteRequestForCurrentProject(request);
            }

            _commandQueue.Enqueue(request);
        }

        private static void ProcessCommandQueue()
        {
            while (_commandQueue.TryDequeue(out var request))
            {
                if (_server == null)
                {
                    continue;
                }

                var existing = StateManager.ReadResultForCurrentProject(request.Id);
                if (existing != null)
                {
                    _server.Send(existing);
                    StateManager.DeleteRequestForCurrentProject(request.Id);
                    continue;
                }

                var response = CommandRouter.Route(request, _settings.ExecuteEnabled);
                StateManager.WriteResultForCurrentProject(request.Id, response);
                _server.Send(response);
                StateManager.DeleteRequestForCurrentProject(request.Id);
            }
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
