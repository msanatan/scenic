using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    internal static class AssetImportSettingsHelpers
    {
        public static AssetImporter GetImporterOrThrow(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null)
            {
                throw new CommandHandlingException($"No importer found for asset: {assetPath}");
            }

            return importer;
        }

        public static JObject ReadProperties(SerializedObject serialized, string[] filter)
        {
            var result = new JObject();

            if (filter != null && filter.Length > 0)
            {
                foreach (var name in filter)
                {
                    var property = serialized.FindProperty(name);
                    if (property != null)
                    {
                        result[name] = ReadPropertyValue(property);
                    }
                }
                return result;
            }

            var iterator = serialized.GetIterator();
            if (!iterator.NextVisible(true))
            {
                return result;
            }

            do
            {
                result[iterator.name] = ReadPropertyValue(iterator);
            }
            while (iterator.NextVisible(false));

            return result;
        }

        public static string[] ApplyProperties(SerializedObject serialized, JObject properties)
        {
            var applied = new List<string>();

            foreach (var kvp in properties)
            {
                var property = serialized.FindProperty(kvp.Key);
                if (property == null)
                {
                    throw new CommandHandlingException($"Property not found on importer: {kvp.Key}");
                }

                WritePropertyValue(property, kvp.Value);
                applied.Add(kvp.Key);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return applied.ToArray();
        }

        private static JToken ReadPropertyValue(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return property.intValue;
                case SerializedPropertyType.Boolean:
                    return property.boolValue;
                case SerializedPropertyType.Float:
                    return property.floatValue;
                case SerializedPropertyType.String:
                    return property.stringValue;
                case SerializedPropertyType.Enum:
                    return property.enumValueIndex;
                default:
                    return property.propertyType.ToString();
            }
        }

        private static void WritePropertyValue(SerializedProperty property, JToken value)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                    property.intValue = value.Value<int>();
                    break;
                case SerializedPropertyType.Boolean:
                    property.boolValue = value.Value<bool>();
                    break;
                case SerializedPropertyType.Float:
                    property.floatValue = value.Value<float>();
                    break;
                case SerializedPropertyType.String:
                    property.stringValue = value.Value<string>();
                    break;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = value.Value<int>();
                    break;
                default:
                    throw new CommandHandlingException(
                        $"Cannot write property '{property.name}' of type {property.propertyType}.");
            }
        }
    }
}
