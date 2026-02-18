using System;
using UnityEngine;

namespace UniBridge.Editor.Commands.GameObject
{
    [UniBridgeCommand("gameobject.create")]
    public sealed class GameObjectCreateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = GameObjectCreateCommandParams.From(request);

            var parentObject = GameObjectLookup.ResolveOptional(parameters.Parent, parameters.ParentInstanceId, "Parent");
            var parent = parentObject == null ? null : parentObject.transform;
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
                Path = GameObjectLookup.BuildPath(go.transform),
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

    }
}
