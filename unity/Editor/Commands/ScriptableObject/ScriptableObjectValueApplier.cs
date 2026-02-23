using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Scenic.Editor.Commands.ScriptableObjects
{
    internal static class ScriptableObjectValueApplier
    {
        public static void Apply(
            UnityEngine.ScriptableObject asset,
            JObject values,
            bool strict,
            List<string> appliedFields,
            List<string> ignoredFields,
            string labelPrefix)
        {
            var type = asset.GetType();
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

                if (assignment.Member is PropertyInfo memberProperty)
                {
                    var converted = ConvertToken(assignment.Token, memberProperty.PropertyType, assignment.Key, labelPrefix);
                    memberProperty.SetValue(asset, converted, null);
                    appliedFields.Add(assignment.Key);
                    continue;
                }

                var memberField = assignment.Member as FieldInfo;
                if (memberField == null)
                {
                    continue;
                }

                var fieldValue = ConvertToken(assignment.Token, memberField.FieldType, assignment.Key, labelPrefix);
                memberField.SetValue(asset, fieldValue);
                appliedFields.Add(assignment.Key);
            }
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
                    return ParseVector2(token, fieldName, labelPrefix);
                }

                if (effectiveType == typeof(Vector3))
                {
                    return ParseVector3(token, fieldName, labelPrefix);
                }

                if (effectiveType == typeof(Vector4))
                {
                    return ParseVector4(token, fieldName, labelPrefix);
                }
            }
            catch (Exception ex)
            {
                throw new CommandHandlingException($"Failed to set {labelPrefix}.{fieldName}: {ex.Message}");
            }

            throw new CommandHandlingException($"Unsupported {labelPrefix} type for field '{fieldName}': {effectiveType.FullName}");
        }

        private static Vector2 ParseVector2(JToken token, string fieldName, string labelPrefix)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"{labelPrefix}.{fieldName} must be an object with x,y.");
            }

            return new Vector2(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.y is required."));
        }

        private static Vector3 ParseVector3(JToken token, string fieldName, string labelPrefix)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"{labelPrefix}.{fieldName} must be an object with x,y,z.");
            }

            return new Vector3(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.y is required."),
                obj.Value<float?>("z") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.z is required."));
        }

        private static Vector4 ParseVector4(JToken token, string fieldName, string labelPrefix)
        {
            if (!(token is JObject obj))
            {
                throw new CommandHandlingException($"{labelPrefix}.{fieldName} must be an object with x,y,z,w.");
            }

            return new Vector4(
                obj.Value<float?>("x") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.x is required."),
                obj.Value<float?>("y") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.y is required."),
                obj.Value<float?>("z") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.z is required."),
                obj.Value<float?>("w") ?? throw new CommandHandlingException($"{labelPrefix}.{fieldName}.w is required."));
        }
    }
}
