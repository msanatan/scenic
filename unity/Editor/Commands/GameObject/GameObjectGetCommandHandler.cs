using UnityEngine;

namespace UniBridge.Editor.Commands.GameObject
{
    [UniBridgeCommand("gameobject.get")]
    public sealed class GameObjectGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = GameObjectGetCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");

            return new GameObjectGetCommandResult
            {
                Name = target.name,
                Path = GameObjectLookup.BuildPath(target.transform),
                InstanceId = target.GetInstanceID(),
                IsActive = target.activeSelf,
                Tag = target.tag,
                Layer = LayerMask.LayerToName(target.layer),
                IsStatic = target.isStatic,
                ParentPath = target.transform.parent == null ? null : GameObjectLookup.BuildPath(target.transform.parent),
                SiblingIndex = target.transform.GetSiblingIndex(),
                Transform = new GameObjectTransformSnapshot
                {
                    Position = new Vector3Value
                    {
                        X = target.transform.localPosition.x,
                        Y = target.transform.localPosition.y,
                        Z = target.transform.localPosition.z,
                    },
                    Rotation = new Vector3Value
                    {
                        X = target.transform.localEulerAngles.x,
                        Y = target.transform.localEulerAngles.y,
                        Z = target.transform.localEulerAngles.z,
                    },
                    Scale = new Vector3Value
                    {
                        X = target.transform.localScale.x,
                        Y = target.transform.localScale.y,
                        Z = target.transform.localScale.z,
                    },
                },
            };
        }
    }
}
