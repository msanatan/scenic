using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Scenic.Editor.Commands.GameObject
{
    internal static class GameObjectLookup
    {
        public static UnityEngine.GameObject ResolveOptional(string path, int? instanceId, string label)
        {
            ValidateSelector(path, instanceId, label);

            if (instanceId.HasValue)
            {
                return ResolveByInstanceId(instanceId.Value, label);
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                return ResolveByPath(path, label);
            }

            return null;
        }

        public static UnityEngine.GameObject ResolveRequired(string path, int? instanceId, string label)
        {
            var value = ResolveOptional(path, instanceId, label);
            if (value != null)
            {
                return value;
            }

            throw new CommandHandlingException($"Missing required parameter: {label} (path or instanceId)");
        }

        public static string BuildPath(Transform transform)
        {
            var names = new Stack<string>();
            var current = transform;
            while (current != null)
            {
                names.Push(current.gameObject.name);
                current = current.parent;
            }

            return "/" + string.Join("/", names.ToArray());
        }

        private static void ValidateSelector(string path, int? instanceId, string label)
        {
            if (!string.IsNullOrWhiteSpace(path) && instanceId.HasValue)
            {
                throw new CommandHandlingException($"Provide either {label}.path or {label}.instanceId, not both.");
            }
        }

        private static UnityEngine.GameObject ResolveByInstanceId(int instanceId, string label)
        {
            var found = EditorUtility.EntityIdToObject(instanceId) as UnityEngine.GameObject;
            if (found == null)
            {
                throw new CommandHandlingException($"{label} GameObject not found for instanceId: {instanceId}");
            }

            return found;
        }

        private static UnityEngine.GameObject ResolveByPath(string path, string label)
        {
            if (!path.StartsWith("/", StringComparison.Ordinal))
            {
                throw new CommandHandlingException($"{label} path must start with '/'.");
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                throw new CommandHandlingException("No active scene is loaded.");
            }

            var matches = new List<UnityEngine.GameObject>();
            var roots = activeScene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                FindByPath(roots[i].transform, path, parentPath: string.Empty, matches);
            }

            if (matches.Count == 0)
            {
                throw new CommandHandlingException($"{label} GameObject not found: {path}");
            }

            if (matches.Count > 1)
            {
                throw new CommandHandlingException($"{label} path is ambiguous: {path}. Use instanceId instead.");
            }

            return matches[0];
        }

        private static void FindByPath(Transform current, string targetPath, string parentPath, List<UnityEngine.GameObject> matches)
        {
            var currentPath = string.IsNullOrWhiteSpace(parentPath)
                ? $"/{current.gameObject.name}"
                : $"{parentPath}/{current.gameObject.name}";

            if (string.Equals(currentPath, targetPath, StringComparison.Ordinal))
            {
                matches.Add(current.gameObject);
            }

            for (var i = 0; i < current.childCount; i++)
            {
                FindByPath(current.GetChild(i), targetPath, currentPath, matches);
            }
        }
    }
}
