using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor.Commands.Material
{
    [ScenicCommand("material.properties.get")]
    public sealed class MaterialPropertiesGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = MaterialPropertiesGetCommandParams.From(request);
            var assetPath = MaterialAssetHelpers.NormalizeAssetPath(parameters.AssetPath, requireExists: true);
            var material = MaterialAssetHelpers.LoadRequired(assetPath);

            var propertyNames = new List<string>();
            if (parameters.Names != null && parameters.Names.Length > 0)
            {
                for (var i = 0; i < parameters.Names.Length; i++)
                {
                    var name = parameters.Names[i];
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    propertyNames.Add(name.Trim());
                }
            }

            if (propertyNames.Count == 0)
            {
                foreach (var name in MaterialPropertyHelpers.GetSupportedPropertyNames(material))
                {
                    propertyNames.Add(name);
                }
            }

            var properties = new JObject();
            for (var i = 0; i < propertyNames.Count; i++)
            {
                var name = propertyNames[i];
                if (!MaterialPropertyHelpers.TryGetPropertyType(material, name, out var propertyType))
                {
                    throw new CommandHandlingException($"Shader property not found on material: {name}");
                }

                if (!MaterialPropertyHelpers.IsSupported(propertyType))
                {
                    throw new CommandHandlingException(
                        $"Unsupported shader property type for '{name}': {propertyType}");
                }

                properties[name] = MaterialPropertyHelpers.ReadValue(material, name, propertyType);
            }

            return new MaterialPropertiesGetCommandResult
            {
                Material = MaterialAssetHelpers.BuildSummary(material, assetPath),
                Properties = properties,
            };
        }
    }
}
