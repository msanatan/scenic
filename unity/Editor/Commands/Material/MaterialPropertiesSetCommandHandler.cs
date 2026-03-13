using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Scenic.Editor.Commands.Material
{
    [ScenicCommand("material.properties.set")]
    public sealed class MaterialPropertiesSetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = MaterialPropertiesSetCommandParams.From(request);
            var assetPath = MaterialAssetHelpers.NormalizeAssetPath(parameters.AssetPath, requireExists: true);
            var material = MaterialAssetHelpers.LoadRequired(assetPath);

            var appliedProperties = new List<string>();
            var ignoredProperties = new List<string>();
            foreach (var property in parameters.Values.Properties())
            {
                var propertyName = property.Name;
                if (!MaterialPropertyHelpers.TryGetPropertyType(material, propertyName, out var propertyType))
                {
                    ignoredProperties.Add(propertyName);
                    continue;
                }

                if (!MaterialPropertyHelpers.IsSupported(propertyType))
                {
                    throw new CommandHandlingException(
                        $"Unsupported shader property type for '{propertyName}': {propertyType}");
                }

                if (!(property.Value is JObject valueSpec))
                {
                    throw new CommandHandlingException($"values.{propertyName} must be an object.");
                }

                MaterialPropertyHelpers.ApplyValue(material, propertyName, valueSpec, propertyType);
                appliedProperties.Add(propertyName);
            }

            if (parameters.Strict && ignoredProperties.Count > 0)
            {
                throw new CommandHandlingException(
                    "Unknown material properties: " + string.Join(", ", ignoredProperties.ToArray()));
            }

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            return new MaterialPropertiesSetCommandResult
            {
                Material = MaterialAssetHelpers.BuildSummary(material, assetPath),
                AppliedProperties = appliedProperties.ToArray(),
                IgnoredProperties = ignoredProperties.ToArray(),
            };
        }
    }
}
