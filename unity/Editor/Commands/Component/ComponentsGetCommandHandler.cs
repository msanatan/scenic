using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UniBridge.Editor.Commands;
using UniBridge.Editor.Commands.GameObject;
using UnityEditor;
using UnityEngine;

namespace UniBridge.Editor.Commands.Components
{
    [UniBridgeCommand("components.get")]
    public sealed class ComponentsGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ComponentsGetCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");
            var all = target.GetComponents<Component>();

            var index = ResolveIndex(all, parameters);
            var component = all[index];
            if (component == null)
            {
                throw new CommandHandlingException($"Component at index {index} is missing.");
            }

            return new ComponentsGetCommandResult
            {
                Component = new ComponentGetItem
                {
                    InstanceId = component.GetInstanceID(),
                    Type = component.GetType().FullName ?? component.GetType().Name,
                    Index = index,
                    Enabled = ReadEnabled(component),
                    Serialized = ReadSerialized(component),
                },
            };
        }

        private static int ResolveIndex(Component[] all, ComponentsGetCommandParams parameters)
        {
            if (parameters.ComponentInstanceId.HasValue)
            {
                for (var i = 0; i < all.Length; i++)
                {
                    var component = all[i];
                    if (component != null && component.GetInstanceID() == parameters.ComponentInstanceId.Value)
                    {
                        return i;
                    }
                }

                throw new CommandHandlingException($"Component not found by instance ID: {parameters.ComponentInstanceId.Value}");
            }

            if (parameters.Index.HasValue)
            {
                var index = parameters.Index.Value;
                if (index < 0 || index >= all.Length)
                {
                    throw new CommandHandlingException($"Component index out of range: {index}");
                }

                return index;
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
                if (typeName.IndexOf(parameters.Type, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matches.Add(i);
                }
            }

            if (matches.Count == 0)
            {
                throw new CommandHandlingException($"Component not found by type: {parameters.Type}");
            }

            if (matches.Count > 1)
            {
                throw new CommandHandlingException($"Multiple components matched type '{parameters.Type}'. Use index or componentInstanceId.");
            }

            return matches[0];
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

        private static JObject ReadSerialized(Component component)
        {
            try
            {
                var json = EditorJsonUtility.ToJson(component, false);
                var token = JToken.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
                return token as JObject ?? new JObject();
            }
            catch
            {
                return new JObject();
            }
        }
    }
}
