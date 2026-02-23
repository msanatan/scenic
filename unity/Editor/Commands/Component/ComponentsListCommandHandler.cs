using System;
using System.Collections.Generic;
using Scenic.Editor.Commands;
using Scenic.Editor.Commands.GameObject;
using UnityEngine;

namespace Scenic.Editor.Commands.Components
{
    [ScenicCommand("components.list")]
    public sealed class ComponentsListCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ComponentsListCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");
            var all = target.GetComponents<Component>();

            var items = new List<ComponentListItem>(all.Length);
            for (var i = 0; i < all.Length; i++)
            {
                var component = all[i];
                if (component == null)
                {
                    continue;
                }

                var typeName = component.GetType().FullName ?? component.GetType().Name;
                if (!string.IsNullOrWhiteSpace(parameters.Type)
                    && typeName.IndexOf(parameters.Type, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                items.Add(new ComponentListItem
                {
                    InstanceId = component.GetInstanceID(),
                    Type = typeName,
                    Index = i,
                    Enabled = ReadEnabled(component),
                });
            }

            var page = Pagination.Slice(items, parameters.Paging, out var total);
            return new ComponentsListCommandResult
            {
                Components = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }

        private static bool? ReadEnabled(Component component)
        {
            var property = component.GetType().GetProperty("enabled");
            if (property == null || property.PropertyType != typeof(bool) || !property.CanRead)
            {
                return null;
            }

            try
            {
                return (bool)property.GetValue(component, null);
            }
            catch
            {
                return null;
            }
        }
    }
}
