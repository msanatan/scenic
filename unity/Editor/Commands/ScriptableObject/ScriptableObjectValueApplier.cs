using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Scenic.Editor.Commands.Common;

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
