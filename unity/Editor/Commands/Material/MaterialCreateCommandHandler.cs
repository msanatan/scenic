using System.IO;
using Scenic.Editor.Commands;
using UnityEditor;

namespace Scenic.Editor.Commands.Material
{
    [ScenicCommand("material.create")]
    public sealed class MaterialCreateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = MaterialCreateCommandParams.From(request);
            var assetPath = MaterialAssetHelpers.NormalizeAssetPath(parameters.AssetPath, requireExists: false);
            if (File.Exists(assetPath))
            {
                throw new CommandHandlingException($"Material asset already exists: {assetPath}");
            }

            var directory = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrWhiteSpace(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                throw new CommandHandlingException($"Directory does not exist: {directory}");
            }

            var shader = MaterialAssetHelpers.ResolveShader(parameters.Shader);
            var material = new UnityEngine.Material(shader)
            {
                name = Path.GetFileNameWithoutExtension(assetPath),
            };

            AssetDatabase.CreateAsset(material, assetPath);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();

            var created = MaterialAssetHelpers.LoadRequired(assetPath);

            return new MaterialCreateCommandResult
            {
                Material = MaterialAssetHelpers.BuildSummary(created, assetPath),
            };
        }
    }
}
