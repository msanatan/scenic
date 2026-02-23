using UnityEngine;

namespace Scenic.Editor.Commands.GameObject
{
    [ScenicCommand("gameobject.destroy")]
    public sealed class GameObjectDestroyCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = GameObjectDestroyCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Target.Path, parameters.Target.InstanceId, "Target");
            var instanceId = target.GetInstanceID();
            var name = target.name;
            var path = GameObjectLookup.BuildPath(target.transform);

            Object.DestroyImmediate(target);

            return new GameObjectDestroyCommandResult
            {
                Destroyed = true,
                Name = name,
                Path = path,
                InstanceId = instanceId,
            };
        }
    }
}
