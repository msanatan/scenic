using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scenic.Editor.Commands.Components
{
    internal static class ComponentSelection
    {
        public static int ResolveIndex(Component[] all, int? componentInstanceId, int? index, string type)
        {
            if (componentInstanceId.HasValue)
            {
                for (var i = 0; i < all.Length; i++)
                {
                    var component = all[i];
                    if (component != null && component.GetInstanceID() == componentInstanceId.Value)
                    {
                        return i;
                    }
                }

                throw new CommandHandlingException($"Component not found by instance ID: {componentInstanceId.Value}");
            }

            if (index.HasValue)
            {
                var selected = index.Value;
                if (selected < 0 || selected >= all.Length)
                {
                    throw new CommandHandlingException($"Component index out of range: {selected}");
                }

                return selected;
            }

            var matches = new List<int>();
            for (var i = 0; i < all.Length; i++)
            {
                var component = all[i];
                if (component == null)
                {
                    continue;
                }

                var typeName = component.GetType().FullName ?? component.GetType().Name;
                if (typeName.IndexOf(type, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matches.Add(i);
                }
            }

            if (matches.Count == 0)
            {
                throw new CommandHandlingException($"Component not found by type: {type}");
            }

            if (matches.Count > 1)
            {
                throw new CommandHandlingException($"Multiple components matched type '{type}'. Use index or componentInstanceId.");
            }

            return matches[0];
        }
    }
}
