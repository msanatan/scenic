using System;
using UnityEngine;

namespace UniBridge.Editor.Commands.GameObject
{
    [UniBridgeCommand("gameobject.update")]
    public sealed class GameObjectUpdateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = GameObjectUpdateCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");

            if (parameters.Name != null)
            {
                target.name = parameters.Name;
            }

            if (parameters.Tag != null)
            {
                try
                {
                    target.tag = parameters.Tag;
                }
                catch (UnityException)
                {
                    throw new CommandHandlingException("Unknown tag: " + parameters.Tag);
                }
            }

            if (parameters.Layer != null)
            {
                target.layer = ResolveLayer(parameters.Layer);
            }

            if (parameters.IsStatic.HasValue)
            {
                target.isStatic = parameters.IsStatic.Value;
            }

            if (parameters.HasTransform)
            {
                ApplyTransform(target.transform, parameters.Transform);
            }

            return new GameObjectUpdateCommandResult
            {
                Name = target.name,
                Path = GameObjectLookup.BuildPath(target.transform),
                InstanceId = target.GetInstanceID(),
                Tag = target.tag,
                Layer = LayerMask.LayerToName(target.layer),
                IsStatic = target.isStatic,
                Transform = new GameObjectTransformSnapshot
                {
                    Position = ToVector3Value(target.transform.localPosition),
                    Rotation = ToVector3Value(target.transform.localEulerAngles),
                    Scale = ToVector3Value(target.transform.localScale),
                },
            };
        }

        private static int ResolveLayer(string layer)
        {
            if (string.IsNullOrWhiteSpace(layer))
            {
                throw new CommandHandlingException("params.layer must not be empty.");
            }

            int parsed;
            if (int.TryParse(layer, out parsed))
            {
                if (parsed < 0 || parsed > 31)
                {
                    throw new CommandHandlingException("params.layer index must be between 0 and 31.");
                }

                return parsed;
            }

            var layerIndex = LayerMask.NameToLayer(layer);
            if (layerIndex < 0)
            {
                throw new CommandHandlingException("Unknown layer: " + layer);
            }

            return layerIndex;
        }

        private static void ApplyTransform(Transform transform, TransformInput input)
        {
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

        private static Vector3Value ToVector3Value(Vector3 value)
        {
            return new Vector3Value
            {
                X = value.x,
                Y = value.y,
                Z = value.z,
            };
        }
    }
}
