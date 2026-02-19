using System.Collections.Generic;
using UniBridge.Editor.Commands;
using UniBridge.Editor.Commands.GameObject;
using UnityEngine;

namespace UniBridge.Editor.Commands.Components
{
    [UniBridgeCommand("components.update")]
    public sealed class ComponentsUpdateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ComponentsUpdateCommandParams.From(request);
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

            var appliedFields = new List<string>();
            var ignoredFields = new List<string>();
            ComponentValueApplier.Apply(
                component,
                parameters.Values,
                parameters.Strict,
                appliedFields,
                ignoredFields,
                "values");

            return new ComponentsUpdateCommandResult
            {
                InstanceId = component.GetInstanceID(),
                Type = component.GetType().FullName ?? component.GetType().Name,
                Index = index,
                AppliedFields = appliedFields.ToArray(),
                IgnoredFields = ignoredFields.ToArray(),
            };
        }
    }
}
