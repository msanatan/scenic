using System.Collections.Generic;
using System.IO;
using Scenic.Editor.Commands;
using UnityEditor;

namespace Scenic.Editor.Commands.ScriptableObjects
{
    [ScenicCommand("scriptableobject.create")]
    public sealed class ScriptableObjectCreateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ScriptableObjectCreateCommandParams.From(request);
            var assetPath = ScriptableObjectAssetHelpers.NormalizeAssetPath(parameters.AssetPath, requireExists: false);
            if (File.Exists(assetPath))
            {
                throw new CommandHandlingException($"ScriptableObject asset already exists: {assetPath}");
            }

            var directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                throw new CommandHandlingException($"Directory does not exist: {directory}");
            }

            var type = ScriptableObjectAssetHelpers.ResolveScriptableObjectType(parameters.Type);
            var asset = UnityEngine.ScriptableObject.CreateInstance(type);
            if (asset == null)
            {
                throw new CommandHandlingException($"Failed to create ScriptableObject instance for type: {parameters.Type}");
            }

            var appliedFields = new List<string>();
            var ignoredFields = new List<string>();
            if (parameters.InitialValues != null)
            {
                try
                {
                    ScriptableObjectValueApplier.Apply(
                        asset,
                        parameters.InitialValues,
                        parameters.Strict,
                        appliedFields,
                        ignoredFields,
                        "initialValues");
                }
                catch
                {
                    UnityEngine.Object.DestroyImmediate(asset);
                    throw;
                }
            }

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            return new ScriptableObjectCreateCommandResult
            {
                Asset = ScriptableObjectAssetHelpers.BuildSummary(asset, assetPath),
                AppliedFields = appliedFields.ToArray(),
                IgnoredFields = ignoredFields.ToArray(),
            };
        }
    }
}
