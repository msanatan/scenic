using Newtonsoft.Json.Linq;
using Scenic.Editor.Commands;
using Scenic.Editor.Commands.GameObject;
using UnityEditor;
using UnityEngine;

namespace Scenic.Editor.Commands.Components
{
    [ScenicCommand("components.get")]
    public sealed class ComponentsGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ComponentsGetCommandParams.From(request);
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
