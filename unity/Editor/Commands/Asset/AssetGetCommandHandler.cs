using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.get")]
    public sealed class AssetGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetGetCommandParams.From(request);
            var guid = AssetDatabase.AssetPathToGUID(parameters.AssetPath);
            var labels = AssetDatabase.GetLabels(
                AssetDatabase.LoadMainAssetAtPath(parameters.AssetPath));
            var dependencies = AssetDatabase.GetDependencies(parameters.AssetPath, false);

            return new AssetGetCommandResult
            {
                AssetPath = parameters.AssetPath,
                Guid = guid,
                Type = AssetPathHelpers.GetAssetTypeName(parameters.AssetPath),
                Name = AssetPathHelpers.GetAssetName(parameters.AssetPath),
                Labels = labels,
                Dependencies = dependencies,
            };
        }
    }
}
