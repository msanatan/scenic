using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.labels.get")]
    public sealed class AssetLabelsGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetLabelsGetCommandParams.From(request);
            var asset = AssetDatabase.LoadMainAssetAtPath(parameters.AssetPath);
            var labels = AssetDatabase.GetLabels(asset);

            return new AssetLabelsGetCommandResult
            {
                AssetPath = parameters.AssetPath,
                Labels = labels,
            };
        }
    }
}
