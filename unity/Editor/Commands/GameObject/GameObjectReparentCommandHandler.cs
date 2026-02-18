using UnityEngine;

namespace UniBridge.Editor.Commands.GameObject
{
    [UniBridgeCommand("gameobject.reparent")]
    public sealed class GameObjectReparentCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = GameObjectReparentCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");
            var parent = parameters.ToRoot
                ? null
                : GameObjectLookup.ResolveRequired(parameters.ParentPath, parameters.ParentInstanceId, "Parent");

            if (parent != null)
            {
                if (target == parent)
                {
                    throw new CommandHandlingException("Target cannot be parented to itself.");
                }

                if (parent.transform.IsChildOf(target.transform))
                {
                    throw new CommandHandlingException("Cannot parent an object under its own descendant.");
                }

                target.transform.SetParent(parent.transform, parameters.WorldPositionStays);
            }
            else
            {
                target.transform.SetParent(null, parameters.WorldPositionStays);
            }

            return new GameObjectReparentCommandResult
            {
                Name = target.name,
                Path = GameObjectLookup.BuildPath(target.transform),
                InstanceId = target.GetInstanceID(),
                ParentPath = target.transform.parent == null ? null : GameObjectLookup.BuildPath(target.transform.parent),
                SiblingIndex = target.transform.GetSiblingIndex(),
            };
        }
    }
}
