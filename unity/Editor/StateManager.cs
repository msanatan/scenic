using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace UniBridge.Editor
{
    public static class StateManager
    {
        private static string _currentProjectHash;

        public static void SetCurrentProjectHash(string hash)
        {
            _currentProjectHash = hash;
        }

        public static string ProjectHash(string projectPath)
        {
            var canonical = CanonicalizeProjectPath(projectPath);
            byte[] bytes;
            using (var sha = SHA256.Create())
            {
                bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
            }
            return BitConverter.ToString(bytes).Replace("-", string.Empty).Substring(0, 12).ToLowerInvariant();
        }

        public static string CanonicalizeProjectPath(string projectPath)
        {
            var fullPath = Path.GetFullPath(projectPath);
            var normalized = fullPath.Replace('\\', '/').TrimEnd('/');

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                normalized = normalized.ToLowerInvariant();
            }

            return normalized;
        }

        public static string BaseStateDirectory()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return Path.Combine(Path.GetTempPath(), "unibridge");
            }

            return "/tmp/unibridge";
        }

        public static string ResolveStateDirectory(string hashOrDirectory)
        {
            if (Path.IsPathRooted(hashOrDirectory) && hashOrDirectory.Contains(Path.DirectorySeparatorChar.ToString()))
            {
                return hashOrDirectory;
            }

            return Path.Combine(BaseStateDirectory(), hashOrDirectory);
        }

        public static void EnsureStateDirectory(string hashOrDirectory)
        {
            var directory = ResolveStateDirectory(hashOrDirectory);
            Directory.CreateDirectory(directory);
            Directory.CreateDirectory(Path.Combine(directory, "requests"));
            Directory.CreateDirectory(Path.Combine(directory, "results"));
        }

        public static void WriteServerJson(
            string hashOrDirectory,
            string projectPath,
            string pluginVersion = "0.0.0",
            int protocolVersion = 1,
            bool executeEnabled = true)
        {
            var directory = ResolveStateDirectory(hashOrDirectory);
            EnsureStateDirectory(directory);

            var payload = JsonConvert.SerializeObject(new
            {
                port = (object)null,
                pid = Process.GetCurrentProcess().Id,
                unityVersion = UnityEngine.Application.unityVersion,
                pluginVersion,
                protocolVersion,
                capabilities = new { executeEnabled },
                projectPath,
            });

            WriteAtomic(Path.Combine(directory, "server.json"), payload);
        }

        public static void WriteResultForCurrentProject(string requestId, CommandResponse response)
        {
            if (string.IsNullOrWhiteSpace(_currentProjectHash))
            {
                throw new InvalidOperationException("Current project hash is not set.");
            }

            response.Id = requestId;
            WriteResult(ResolveStateDirectory(_currentProjectHash), response);
        }

        public static void WriteRequestForCurrentProject(CommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(_currentProjectHash))
            {
                throw new InvalidOperationException("Current project hash is not set.");
            }

            WriteRequest(ResolveStateDirectory(_currentProjectHash), request);
        }

        public static void WriteResult(string stateDirectory, CommandResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.Id))
            {
                throw new ArgumentException("Response id is required", nameof(response));
            }

            if (response.Id.Contains("/") || response.Id.Contains("\\"))
            {
                throw new ArgumentException("Response id contains invalid path characters", nameof(response));
            }

            EnsureStateDirectory(stateDirectory);
            var resultsDir = Path.Combine(ResolveStateDirectory(stateDirectory), "results");
            var resultPath = Path.Combine(resultsDir, $"{response.Id}.json");
            WriteAtomic(resultPath, response.ToJson());
        }

        public static void WriteRequest(string stateDirectory, CommandRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            ValidateRequestId(request.Id);
            EnsureStateDirectory(stateDirectory);
            var requestsDir = Path.Combine(ResolveStateDirectory(stateDirectory), "requests");
            var requestPath = Path.Combine(requestsDir, $"{request.Id}.json");
            WriteAtomic(requestPath, request.ToJson());
        }

        public static CommandResponse ReadResult(string stateDirectory, string requestId)
        {
            var path = Path.Combine(ResolveStateDirectory(stateDirectory), "results", $"{requestId}.json");
            if (!File.Exists(path))
            {
                return null;
            }

            var content = File.ReadAllText(path);
            if (!CommandResponse.TryParse(content, out var response))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(response.Id))
            {
                response.Id = requestId;
            }

            return response;
        }

        public static CommandResponse ReadResultForCurrentProject(string requestId)
        {
            if (string.IsNullOrWhiteSpace(_currentProjectHash))
            {
                throw new InvalidOperationException("Current project hash is not set.");
            }

            return ReadResult(ResolveStateDirectory(_currentProjectHash), requestId);
        }

        public static CommandRequest ReadRequest(string stateDirectory, string requestId)
        {
            ValidateRequestId(requestId);
            var path = Path.Combine(ResolveStateDirectory(stateDirectory), "requests", $"{requestId}.json");
            if (!File.Exists(path))
            {
                return null;
            }

            var content = File.ReadAllText(path);
            return CommandRequest.TryParse(content, out var request) ? request : null;
        }

        public static CommandRequest[] ListPendingRequests(string stateDirectory)
        {
            EnsureStateDirectory(stateDirectory);
            var directory = ResolveStateDirectory(stateDirectory);
            var requestsDir = Path.Combine(directory, "requests");
            var resultsDir = Path.Combine(directory, "results");
            var requestPaths = Directory.GetFiles(requestsDir, "*.json");
            Array.Sort(requestPaths, StringComparer.Ordinal);

            var pending = new List<CommandRequest>(requestPaths.Length);
            foreach (var requestPath in requestPaths)
            {
                var requestId = Path.GetFileNameWithoutExtension(requestPath);
                var resultPath = Path.Combine(resultsDir, requestId + ".json");
                if (File.Exists(resultPath))
                {
                    continue;
                }

                var content = File.ReadAllText(requestPath);
                if (CommandRequest.TryParse(content, out var request))
                {
                    pending.Add(request);
                }
            }

            return pending.ToArray();
        }

        public static CommandRequest[] ListPendingRequestsForCurrentProject()
        {
            if (string.IsNullOrWhiteSpace(_currentProjectHash))
            {
                throw new InvalidOperationException("Current project hash is not set.");
            }

            return ListPendingRequests(ResolveStateDirectory(_currentProjectHash));
        }

        public static void DeleteResult(string stateDirectory, string requestId)
        {
            var path = Path.Combine(ResolveStateDirectory(stateDirectory), "results", $"{requestId}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void DeleteRequest(string stateDirectory, string requestId)
        {
            ValidateRequestId(requestId);
            var path = Path.Combine(ResolveStateDirectory(stateDirectory), "requests", $"{requestId}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void DeleteRequestForCurrentProject(string requestId)
        {
            if (string.IsNullOrWhiteSpace(_currentProjectHash))
            {
                throw new InvalidOperationException("Current project hash is not set.");
            }

            DeleteRequest(ResolveStateDirectory(_currentProjectHash), requestId);
        }

        public static string CurrentStateDirectory()
        {
            if (string.IsNullOrWhiteSpace(_currentProjectHash))
            {
                throw new InvalidOperationException("Current project hash is not set.");
            }

            return ResolveStateDirectory(_currentProjectHash);
        }

        public static void Cleanup()
        {
            // Intentionally keep state directory for crash/reload resilience.
        }

        private static void WriteAtomic(string destinationPath, string content)
        {
            var tempPath = destinationPath + ".tmp";
            File.WriteAllText(tempPath, content);

            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            File.Move(tempPath, destinationPath);
        }

        private static void ValidateRequestId(string requestId)
        {
            if (string.IsNullOrWhiteSpace(requestId))
            {
                throw new ArgumentException("Request id is required", nameof(requestId));
            }

            if (requestId.Contains("/") || requestId.Contains("\\"))
            {
                throw new ArgumentException("Request id contains invalid path characters", nameof(requestId));
            }
        }
    }
}
