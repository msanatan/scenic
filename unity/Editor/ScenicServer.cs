using System.Collections.Concurrent;
using System.IO;
using UnityEditor;
using UnityEngine;
using SettingsService = Scenic.Editor.Settings.SettingsService;
using SettingsRuntime = Scenic.Editor.Settings.SettingsRuntime;

namespace Scenic.Editor
{
    [InitializeOnLoad]
    public static class ScenicServer
    {
        private static PipeServer _server;
        private static SettingsService _settingsService;
        private static string _projectHash;
        private static readonly ConcurrentQueue<CommandRequest> _commandQueue = new ConcurrentQueue<CommandRequest>();
        private const string SessionResultPrefix = "scenic_result_";

        static ScenicServer()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeReload;
            EditorApplication.quitting += OnQuit;
            EditorApplication.update += ProcessCommandQueue;

            Start();
        }

        private static void Start()
        {
            var projectPath = Path.GetDirectoryName(Application.dataPath);
            _projectHash = StateManager.ProjectHash(projectPath);

            StateManager.SetCurrentProjectHash(_projectHash);
            StateManager.EnsureStateDirectory(_projectHash);
            _settingsService = new SettingsService();
            _settingsService.Initialize(_projectHash);
            SettingsRuntime.SetService(_settingsService);
            var pluginVersion = PluginVersion.Get();
            StateManager.WriteServerJson(
                _projectHash,
                projectPath,
                protocolVersion: 1,
                pluginVersion: pluginVersion);

            _server = new PipeServer(_projectHash);
            _server.OnCommandReceived += OnCommand;
            _server.Start();
            UnityEngine.Debug.Log($"ScenicServer started for project '{projectPath}' with hash '{_projectHash}'.");
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
                    try
                    {
                        _server.Send(cachedResponse);
                        UnityEngine.Debug.Log($"Sent cached response for id: {request.Id}");
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Failed to send cached response for id: {request.Id}. {ex.Message}");
                    }
                    continue;
                }

                var executeEnabled = _settingsService.Get().ExecuteEnabled;
                var response = CommandRouter.Route(request, executeEnabled);
                CacheResponse(response);
                UnityEngine.Debug.Log($"Completed command: {request.Command} id: {request.Id} success: {response.Success}");

                try
                {
                    _server.Send(response);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to send response for command id: {request.Id}. {ex.Message}");
                }
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
            SettingsRuntime.ClearService();
        }

        private static void OnQuit()
        {
            _server?.Stop();
            SettingsRuntime.ClearService();
        }
    }
}
