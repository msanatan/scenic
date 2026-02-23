using Scenic.Editor.Commands;
using Scenic.Editor.Commands.GameObject;
using UnityEngine;

namespace Scenic.Editor.Commands.Components
{
    [ScenicCommand("components.remove")]
    public sealed class ComponentsRemoveCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ComponentsRemoveCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");
            var all = target.GetComponents<Component>();

            var index = ComponentSelection.ResolveIndex(
                all,
                parameters.ComponentInstanceId,
                parameters.Index,
                parameters.Type);
            var component = all[index];
            if (component == null)
            {
                throw new CommandHandlingException($"Component at index {index} is missing.");
            }

            if (component is Transform)
            {
                throw new CommandHandlingException("Cannot remove Transform component.");
            }

            var instanceId = component.GetInstanceID();
            var type = component.GetType().FullName ?? component.GetType().Name;
            Object.DestroyImmediate(component);

            return new ComponentsRemoveCommandResult
            {
                Removed = true,
                InstanceId = instanceId,
                Type = type,
                Index = index,
            };
        }
    }
}
