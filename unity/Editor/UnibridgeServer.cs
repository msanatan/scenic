using System.Collections.Concurrent;
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
        private const string SessionResultPrefix = "unibridge_result_";

        static UniBridgeServer()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
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
            UnityEngine.Debug.Log($"UniBridgeServer started for project '{projectPath}' with hash '{_projectHash}'.");
        }

        private static void OnCommand(CommandRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Id))
            {
                return;
            }

            UnityEngine.Debug.Log($"Received command: {request.Command} with id: {request.Id}");
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

                if (TryGetCachedResponse(request.Id, out var cachedResponse))
                {
                    _server.Send(cachedResponse);
                    continue;
                }

                var response = CommandRouter.Route(request, _settings.ExecuteEnabled);
                CacheResponse(response);
                _server.Send(response);
            }
        }

        private static void CacheResponse(CommandResponse response)
        {
            if (response == null || string.IsNullOrWhiteSpace(response.Id))
            {
                return;
            }

            SessionState.SetString(SessionResultPrefix + response.Id, response.ToJson());
        }

        private static bool TryGetCachedResponse(string requestId, out CommandResponse response)
        {
            response = null;
            if (string.IsNullOrWhiteSpace(requestId))
            {
                return false;
            }

            var cached = SessionState.GetString(SessionResultPrefix + requestId, null);
            if (string.IsNullOrWhiteSpace(cached))
            {
                return false;
            }

            if (!CommandResponse.TryParse(cached, out var parsed))
            {
                return false;
            }

            response = parsed;
            return true;
        }

        private static void OnBeforeReload()
        {
            _server?.Stop();
        }

        private static void OnQuit()
        {
            _server?.Stop();
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
