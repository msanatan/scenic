using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scenic.Editor.Settings;

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
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(localAppData, "scenic");
            }

            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".scenic");
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
            int protocolVersion = 1)
        {
            var directory = ResolveStateDirectory(hashOrDirectory);
            EnsureStateDirectory(directory);

            var payload = JsonConvert.SerializeObject(new
            {
                pid = Process.GetCurrentProcess().Id,
                unityVersion = UnityEngine.Application.unityVersion,
                pluginVersion,
                protocolVersion,
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
            return ReadSettingsOrDefault(hashOrDirectory).ExecuteEnabled;
        }

        public static ScenicSettingsModel ReadSettingsOrDefault(string hashOrDirectory)
        {
            var directory = ResolveStateDirectory(hashOrDirectory);

            var configPath = Path.Combine(directory, "config.json");
            if (File.Exists(configPath))
            {
                try
                {
                    var json = JObject.Parse(File.ReadAllText(configPath));
                    return new ScenicSettingsModel
                    {
                        ExecuteEnabled = json["executeEnabled"]?.Value<bool>() == true,
                    };
                }
                catch
                {
                    return ScenicSettingsModel.Default();
                }
            }

            // Fallback: read from server.json for backward compat with older CLI/SDK
            var legacyPath = Path.Combine(directory, "server.json");
            if (File.Exists(legacyPath))
            {
                try
                {
                    var json = JObject.Parse(File.ReadAllText(legacyPath));
                    return new ScenicSettingsModel
                    {
                        ExecuteEnabled = json["capabilities"]?["executeEnabled"]?.Value<bool>() == true,
                    };
                }
                catch
                {
                    return ScenicSettingsModel.Default();
                }
            }

            return ScenicSettingsModel.Default();
        }

        public static void WriteSettings(string hashOrDirectory, ScenicSettingsModel settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var directory = ResolveStateDirectory(hashOrDirectory);
            EnsureStateDirectory(directory);

            var target = Path.Combine(directory, "config.json");
            var temp = target + ".tmp";
            var payload = JsonConvert.SerializeObject(settings, Formatting.Indented) + Environment.NewLine;

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
