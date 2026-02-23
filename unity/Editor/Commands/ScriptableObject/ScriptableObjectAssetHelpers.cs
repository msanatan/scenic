using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Scenic.Editor.Commands;
using UnityEditor;

namespace Scenic.Editor.Commands.ScriptableObjects
{
    internal static class ScriptableObjectAssetHelpers
    {
        public static string NormalizeAssetPath(string value, bool requireExists)
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace('\\', '/').Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new CommandHandlingException("assetPath is required.");
            }

            if (Path.IsPathRooted(normalized))
            {
                throw new CommandHandlingException("Absolute paths are not supported. Use a project-relative assetPath.");
            }

            if (!normalized.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new CommandHandlingException("assetPath must start with 'Assets/'.");
            }

            if (!normalized.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            {
                throw new CommandHandlingException("assetPath must end with '.asset'.");
            }

            if (requireExists && !File.Exists(normalized))
            {
                throw new CommandHandlingException($"ScriptableObject asset does not exist: {normalized}");
            }

            return normalized;
        }

        public static Type ResolveScriptableObjectType(string value)
        {
            var matches = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly =>
                {
                    try
                    {
                        return assembly.GetType(value, false, true);
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(type => type != null)
                .ToArray();

            if (matches.Length == 0)
            {
                throw new CommandHandlingException($"Unknown ScriptableObject type: {value}");
            }

            var selected = matches[0];
            if (!typeof(UnityEngine.ScriptableObject).IsAssignableFrom(selected))
            {
                throw new CommandHandlingException($"Type is not a ScriptableObject: {value}");
            }

            if (selected.IsAbstract)
            {
                throw new CommandHandlingException($"Type is abstract and cannot be instantiated: {value}");
            }

            return selected;
        }

        public static UnityEngine.ScriptableObject LoadRequired(string assetPath)
        {
            var loaded = AssetDatabase.LoadAssetAtPath<UnityEngine.ScriptableObject>(assetPath);
            if (loaded == null)
            {
                throw new CommandHandlingException($"Failed to load ScriptableObject asset: {assetPath}");
            }

            return loaded;
        }

        public static ScriptableObjectSummary BuildSummary(UnityEngine.ScriptableObject asset, string assetPath)
        {
            var type = asset.GetType();
            return new ScriptableObjectSummary
            {
                AssetPath = assetPath,
                Name = asset.name,
                Type = type.FullName ?? type.Name,
                InstanceId = asset.GetInstanceID(),
            };
        }

        public static JObject ReadSerialized(UnityEngine.ScriptableObject asset)
        {
            try
            {
                var json = EditorJsonUtility.ToJson(asset, false);
                var token = JToken.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
                return token as JObject ?? new JObject();
            }
            catch
            {
                return new JObject();
            }
        }
    }
}
