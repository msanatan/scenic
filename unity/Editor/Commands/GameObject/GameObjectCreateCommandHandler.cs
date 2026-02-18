using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UniBridge.Editor.Commands.GameObject
{
    [UniBridgeCommand("gameobject.create")]
    public sealed class GameObjectCreateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = GameObjectCreateCommandParams.From(request);

            var parent = ResolveTransform(parameters.Parent, parameters.ParentInstanceId);
            var go = CreateBaseObject(parameters);
            go.name = parameters.Name;

            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            ApplyTransform(go.transform, parameters.Transform);

            return new GameObjectCreateCommandResult
            {
                Name = go.name,
                Path = BuildPath(go.transform),
                IsActive = go.activeSelf,
                SiblingIndex = go.transform.GetSiblingIndex(),
                InstanceId = go.GetInstanceID(),
            };
        }

        private static UnityEngine.GameObject CreateBaseObject(GameObjectCreateCommandParams parameters)
        {
            if (!string.IsNullOrWhiteSpace(parameters.Primitive))
            {
                var primitive = MapPrimitive(parameters.Primitive);
                return UnityEngine.GameObject.CreatePrimitive(primitive);
            }

            var go = new UnityEngine.GameObject(parameters.Name);
            if (parameters.Dimension == "2d" && go.GetComponent<SpriteRenderer>() == null)
            {
                go.AddComponent<SpriteRenderer>();
            }

            return go;
        }

        private static PrimitiveType MapPrimitive(string primitive)
        {
            switch (primitive)
            {
                case "cube":
                    return PrimitiveType.Cube;
                case "sphere":
                    return PrimitiveType.Sphere;
                case "capsule":
                    return PrimitiveType.Capsule;
                case "cylinder":
                    return PrimitiveType.Cylinder;
                case "plane":
                    return PrimitiveType.Plane;
                case "quad":
                    return PrimitiveType.Quad;
                default:
                    throw new CommandHandlingException($"Unsupported primitive: {primitive}");
            }
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

        private static string BuildPath(Transform transform)
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

        private static Transform ResolveTransform(string path, int? instanceId)
        {
            if (instanceId.HasValue)
            {
                var parentById = EditorUtility.EntityIdToObject(instanceId.Value) as UnityEngine.GameObject;
                if (parentById == null)
                {
                    throw new CommandHandlingException($"Parent GameObject not found for instanceId: {instanceId.Value}");
                }

                return parentById.transform;
            }

            return ResolveTransformByPath(path);
        }

        private static Transform ResolveTransformByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (!path.StartsWith("/", StringComparison.Ordinal))
            {
                throw new CommandHandlingException("Parent path must start with '/'.");
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                throw new CommandHandlingException("No active scene is loaded.");
            }

            var roots = activeScene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var match = FindByPath(roots[i].transform, path, parentPath: string.Empty);
                if (match != null)
                {
                    return match;
                }
            }

            throw new CommandHandlingException($"Parent GameObject not found: {path}");
        }

        private static Transform FindByPath(Transform current, string targetPath, string parentPath)
        {
            var currentPath = string.IsNullOrWhiteSpace(parentPath)
                ? $"/{current.gameObject.name}"
                : $"{parentPath}/{current.gameObject.name}";

            if (string.Equals(currentPath, targetPath, StringComparison.Ordinal))
            {
                return current;
            }

            for (var i = 0; i < current.childCount; i++)
            {
                var child = current.GetChild(i);
                var found = FindByPath(child, targetPath, currentPath);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
