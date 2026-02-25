using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor
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
                return Path.Combine(Path.GetTempPath(), "scenic");
            }

            return "/tmp/scenic";
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
        }

        public static void WriteServerJson(
            string hashOrDirectory,
            string projectPath,
            string pluginVersion = "0.0.0",
            int protocolVersion = 1,
            bool executeEnabled = false)
        {
            var directory = ResolveStateDirectory(hashOrDirectory);
            EnsureStateDirectory(directory);

            var payload = JsonConvert.SerializeObject(new
            {
                pid = Process.GetCurrentProcess().Id,
                unityVersion = UnityEngine.Application.unityVersion,
                pluginVersion,
                protocolVersion,
                capabilities = new { executeEnabled },
                projectPath,
            });

            var target = Path.Combine(directory, "server.json");
            var temp = target + ".tmp";
            File.WriteAllText(temp, payload);
            if (File.Exists(target))
            {
                File.Replace(temp, target, null);
            }
            else
            {
                File.Move(temp, target);
            }
        }

        public static bool ReadExecuteEnabled(string hashOrDirectory)
        {
            var directory = ResolveStateDirectory(hashOrDirectory);
            var path = Path.Combine(directory, "server.json");
            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                var json = JObject.Parse(File.ReadAllText(path));
                return json["capabilities"]?["executeEnabled"]?.Value<bool>() == true;
            }
            catch
            {
                return false;
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
    }
}
