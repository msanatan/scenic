using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Scenic.Editor.Commands.Common;

namespace Scenic.Editor.Commands.Components
{
    internal static class ComponentValueApplier
    {
        public static void Apply(
            Component component,
            JObject values,
            bool strict,
            List<string> appliedFields,
            List<string> ignoredFields,
            string labelPrefix)
        {
            var type = component.GetType();
            var assignments = new List<(string Key, JToken Token, MemberInfo Member)>();

            foreach (var property in values.Properties())
            {
                var key = property.Name;
                var token = property.Value;

                var memberProperty = type.GetProperty(
                    key,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (memberProperty != null && memberProperty.CanWrite)
                {
                    assignments.Add((key, token, memberProperty));
                    continue;
                }

                var memberField = type.GetField(
                    key,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
                if (memberField != null)
                {
                    assignments.Add((key, token, memberField));
                    continue;
                }

                ignoredFields.Add(key);
            }

            if (strict && ignoredFields.Count > 0)
            {
                throw new CommandHandlingException($"Unknown {labelPrefix} fields: " + string.Join(", ", ignoredFields.ToArray()));
            }

            for (var i = 0; i < assignments.Count; i++)
            {
                var assignment = assignments[i];
                var key = assignment.Key;
                var token = assignment.Token;
                var member = assignment.Member;

                if (member is PropertyInfo memberProperty)
                {
                    var converted = ConvertToken(token, memberProperty.PropertyType, key, labelPrefix);
                    memberProperty.SetValue(component, converted, null);
                    appliedFields.Add(key);
                    continue;
                }

                var memberField = member as FieldInfo;
                if (memberField == null)
                {
                    continue;
                }

                var fieldValue = ConvertToken(token, memberField.FieldType, key, labelPrefix);
                memberField.SetValue(component, fieldValue);
                appliedFields.Add(key);
            }
        }

        public static Type ResolveComponentType(string value)
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

        private static object ConvertToken(JToken token, Type targetType, string fieldName, string labelPrefix)
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
                    return VectorParsingHelpers.ParseVector2(token, fieldName, labelPrefix);
                }

                if (effectiveType == typeof(Vector3))
                {
                    return VectorParsingHelpers.ParseVector3(token, fieldName, labelPrefix);
                }

                if (effectiveType == typeof(Vector4))
                {
                    return VectorParsingHelpers.ParseVector4(token, fieldName, labelPrefix);
                }
            }
            catch (Exception ex)
            {
                throw new CommandHandlingException($"Failed to set {labelPrefix}.{fieldName}: {ex.Message}");
            }

            throw new CommandHandlingException($"Unsupported {labelPrefix} type for field '{fieldName}': {effectiveType.FullName}");
        }

    }
}
