using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UniBridge.Editor.Commands;
using UniBridge.Editor.Commands.GameObject;
using UnityEngine;

namespace UniBridge.Editor.Commands.Components
{
    [UniBridgeCommand("components.add")]
    public sealed class ComponentsAddCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ComponentsAddCommandParams.From(request);
            var target = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");
            var componentType = ResolveComponentType(parameters.Type);

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
                ApplyInitialValues(component, parameters.InitialValues, parameters.Strict, appliedFields, ignoredFields);
            }

            return new ComponentsAddCommandResult
            {
                InstanceId = component.GetInstanceID(),
                Type = component.GetType().FullName ?? component.GetType().Name,
                AppliedFields = appliedFields.ToArray(),
                IgnoredFields = ignoredFields.ToArray(),
            };
        }

        private static Type ResolveComponentType(string value)
        {
            var candidates = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (var i = 0; i < assemblies.Length; i++)
            {
                Type type = null;
                try
                {
                    type = assemblies[i].GetType(value, false, true);
                }
                catch
                {
                    type = null;
                }

                if (type == null)
                {
                    continue;
                }

                candidates.Add(type);
            }

            if (candidates.Count == 0)
            {
                throw new CommandHandlingException($"Unknown component type: {value}");
            }

            var selected = candidates[0];
            if (!typeof(Component).IsAssignableFrom(selected))
            {
                throw new CommandHandlingException($"Type is not a Unity Component: {value}");
            }

            return selected;
        }

        private static void ApplyInitialValues(
            Component component,
            JObject values,
            bool strict,
            List<string> appliedFields,
            List<string> ignoredFields)
        {
            var type = component.GetType();
            foreach (var property in values.Properties())
            {
                var key = property.Name;
                var token = property.Value;

                var memberProperty = type.GetProperty(
                    key,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (memberProperty != null && memberProperty.CanWrite)
                {
                    var converted = ConvertToken(token, memberProperty.PropertyType, key);
                    memberProperty.SetValue(component, converted, null);
                    appliedFields.Add(key);
                    continue;
                }

                var memberField = type.GetField(
                    key,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (memberField != null)
                {
                    var converted = ConvertToken(token, memberField.FieldType, key);
                    memberField.SetValue(component, converted);
                    appliedFields.Add(key);
                    continue;
                }

                ignoredFields.Add(key);
            }

            if (strict && ignoredFields.Count > 0)
            {
                UnityEngine.Object.DestroyImmediate(component);
                throw new CommandHandlingException("Unknown initialValues fields: " + string.Join(", ", ignoredFields.ToArray()));
            }
        }

        private static object ConvertToken(JToken token, Type targetType, string fieldName)
        {
            var effectiveType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (effectiveType == typeof(string))
                {
                    return token.Type == JTokenType.Null ? null : token.Value<string>();
                }

                if (effectiveType == typeof(bool))
                {
                    return token.Value<bool>();
                }

                if (effectiveType == typeof(int))
                {
                    return token.Value<int>();
                }

                if (effectiveType == typeof(long))
                {
                    return token.Value<long>();
                }

                if (effectiveType == typeof(float))
                {
                    return token.Value<float>();
                }

                if (effectiveType == typeof(double))
                {
                    return token.Value<double>();
                }

                if (effectiveType.IsEnum)
                {
                    if (token.Type == JTokenType.String)
                    {
                        return Enum.Parse(effectiveType, token.Value<string>(), ignoreCase: true);
                    }

                    var numericValue = token.Value<int>();
                    return Enum.ToObject(effectiveType, numericValue);
                }

                if (effectiveType == typeof(Vector2))
                {
                    return ParseVector2(token, fieldName);
                }

                if (effectiveType == typeof(Vector3))
                {
                    return ParseVector3(token, fieldName);
                }

                if (effectiveType == typeof(Vector4))
                {
                    return ParseVector4(token, fieldName);
                }
            }
            catch (Exception ex)
            {
                throw new CommandHandlingException($"Failed to set initialValues.{fieldName}: {ex.Message}");
            }

            throw new CommandHandlingException($"Unsupported initial value type for field '{fieldName}': {effectiveType.FullName}");
        }

        private static Vector2 ParseVector2(JToken token, string fieldName)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"initialValues.{fieldName} must be an object with x,y.");
            }

            return new Vector2(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"initialValues.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"initialValues.{fieldName}.y is required."));
        }

        private static Vector3 ParseVector3(JToken token, string fieldName)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"initialValues.{fieldName} must be an object with x,y,z.");
            }

            return new Vector3(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"initialValues.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"initialValues.{fieldName}.y is required."),
                obj.Value<float?>("z") ?? throw new CommandHandlingException($"initialValues.{fieldName}.z is required."));
        }

        private static Vector4 ParseVector4(JToken token, string fieldName)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"initialValues.{fieldName} must be an object with x,y,z,w.");
            }

            return new Vector4(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"initialValues.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"initialValues.{fieldName}.y is required."),
                obj.Value<float?>("z") ?? throw new CommandHandlingException($"initialValues.{fieldName}.z is required."),
                obj.Value<float?>("w") ?? throw new CommandHandlingException($"initialValues.{fieldName}.w is required."));
        }
    }
}
