using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
            Directory.CreateDirectory(Path.Combine(directory, "results"));
        }

        public static void WriteServerJson(
            string hashOrDirectory,
            string projectPath,
            int protocolVersion = 1,
            bool executeEnabled = true,
            string pluginVersion = "0.1.0")
        {
            var directory = ResolveStateDirectory(hashOrDirectory);
            EnsureStateDirectory(directory);

            var payload =
            "{" +
            "\"port\":null," +
            "\"pid\":" + Process.GetCurrentProcess().Id + "," +
            "\"unityVersion\":" + JsonCompat.Quote(UnityEngine.Application.unityVersion) + "," +
            "\"pluginVersion\":" + JsonCompat.Quote(pluginVersion) + "," +
            "\"protocolVersion\":" + protocolVersion + "," +
            "\"capabilities\":{\"executeEnabled\":" + (executeEnabled ? "true" : "false") + "}," +
            "\"projectPath\":" + JsonCompat.Quote(projectPath) +
            "}";

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

        public static CommandResponse ReadResult(string stateDirectory, string requestId)
        {
            var path = Path.Combine(ResolveStateDirectory(stateDirectory), "results", $"{requestId}.json");
            if (!File.Exists(path))
            {
                return null;
            }

            var content = File.ReadAllText(path);
            return new CommandResponse
            {
                Id = JsonCompat.ExtractString(content, "id") ?? requestId,
                Success = string.Equals(JsonCompat.ExtractString(content, "success"), "true", StringComparison.OrdinalIgnoreCase)
                    || content.Contains("\"success\":true"),
                Result = JsonCompat.ExtractString(content, "result"),
                Error = JsonCompat.ExtractString(content, "error"),
            };
        }

        public static void DeleteResult(string stateDirectory, string requestId)
        {
            var path = Path.Combine(ResolveStateDirectory(stateDirectory), "results", $"{requestId}.json");
            if (File.Exists(path))
            {
                File.Delete(path);
            }
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
    }
}
