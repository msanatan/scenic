using System;
using System.IO;
using UnityEditor;
using UniBridge.Editor.Commands.GameObject;
using UnityEngine;

namespace UniBridge.Editor.Commands.Prefab
{
    [UniBridgeCommand("prefab.instantiate")]
    public sealed class PrefabInstantiateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = PrefabInstantiateCommandParams.From(request);
            var prefabPath = NormalizePrefabPath(parameters.PrefabPath, requireExists: true);
            var prefabAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(prefabPath);
            if (prefabAsset == null)
            {
                throw new CommandHandlingException($"Prefab asset not found: {prefabPath}");
            }

            var instance = PrefabUtility.InstantiatePrefab(prefabAsset) as UnityEngine.GameObject;
            if (instance == null)
            {
                throw new CommandHandlingException($"Failed to instantiate prefab: {prefabPath}");
            }

            var parentObject = GameObjectLookup.ResolveOptional(parameters.ParentPath, parameters.ParentInstanceId, "Parent");
            if (parentObject != null)
            {
                instance.transform.SetParent(parentObject.transform, false);
            }

            ApplyTransform(instance.transform, parameters.Transform);

            return new PrefabInstantiateCommandResult
            {
                PrefabPath = prefabPath,
                Name = instance.name,
                Path = GameObjectLookup.BuildPath(instance.transform),
                InstanceId = instance.GetInstanceID(),
                SiblingIndex = instance.transform.GetSiblingIndex(),
                IsActive = instance.activeSelf,
                Transform = new TransformSnapshot
                {
                    Position = ToVector3Output(instance.transform.localPosition),
                    Rotation = ToVector3Output(instance.transform.localEulerAngles),
                    Scale = ToVector3Output(instance.transform.localScale),
                },
            };
        }

        private static string NormalizePrefabPath(string path, bool requireExists)
        {
            var normalized = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/').Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new CommandHandlingException("Missing required parameter: prefabPath");
            }

            if (Path.IsPathRooted(normalized))
            {
                throw new CommandHandlingException("Absolute paths are not supported. Use a project-relative prefab path.");
            }

            if (!normalized.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new CommandHandlingException("prefabPath must start with 'Assets/'.");
            }

            if (!normalized.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new CommandHandlingException("prefabPath must end with '.prefab'.");
            }

            if (requireExists && !File.Exists(normalized))
            {
                throw new CommandHandlingException($"Prefab does not exist: {normalized}");
            }

            return normalized;
        }

        private static void ApplyTransform(Transform transform, TransformInput input)
        {
            if (input == null)
            {
                return;
            }

            var isWorld = string.Equals(input.Space, "world", StringComparison.Ordinal);

            if (input.Position != null && input.Position.HasValue)
            {
                var value = new Vector3(input.Position.X, input.Position.Y, input.Position.Z);
                if (isWorld)
                {
                    transform.position = value;
                }
                else
                {
                    transform.localPosition = value;
                }
            }

            if (input.Rotation != null && input.Rotation.HasValue)
            {
                var euler = new Vector3(input.Rotation.X, input.Rotation.Y, input.Rotation.Z);
                if (isWorld)
                {
                    transform.rotation = Quaternion.Euler(euler);
                }
                else
                {
                    transform.localRotation = Quaternion.Euler(euler);
                }
            }

            if (input.Scale != null && input.Scale.HasValue)
            {
                transform.localScale = new Vector3(input.Scale.X, input.Scale.Y, input.Scale.Z);
            }
        }

        private static Vector3Output ToVector3Output(Vector3 value)
        {
            return new Vector3Output
            {
                X = value.x,
                Y = value.y,
                Z = value.z,
            };
        }
    }
}
