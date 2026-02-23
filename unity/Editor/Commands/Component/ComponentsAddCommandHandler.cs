using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Scenic.Editor.Commands;
using Scenic.Editor.Commands.GameObject;
using UnityEngine;

namespace Scenic.Editor.Commands.Components
{
    [ScenicCommand("components.add")]
    public sealed class ComponentsAddCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ComponentsAddCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");
            var componentType = ComponentValueApplier.ResolveComponentType(parameters.Type);

            Component component;
            try
            {
                component = target.AddComponent(componentType);
            }
            catch (Exception ex)
            {
                throw new CommandHandlingException($"Failed to add component '{parameters.Type}': {ex.Message}");
            }

            var appliedFields = new List<string>();
            var ignoredFields = new List<string>();
            if (parameters.InitialValues != null)
            {
                try
                {
                    ComponentValueApplier.Apply(
                        component,
                        parameters.InitialValues,
                        parameters.Strict,
                        appliedFields,
                        ignoredFields,
                        "initialValues");
                }
                catch
                {
                    UnityEngine.Object.DestroyImmediate(component);
                    throw;
                }
            }

            return new ComponentsAddCommandResult
            {
                InstanceId = component.GetInstanceID(),
                Type = component.GetType().FullName ?? component.GetType().Name,
                AppliedFields = appliedFields.ToArray(),
                IgnoredFields = ignoredFields.ToArray(),
            };
        }
    }
}
