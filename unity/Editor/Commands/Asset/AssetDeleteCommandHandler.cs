using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.delete")]
    public sealed class AssetDeleteCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetDeleteCommandParams.From(request);
            var deleted = AssetDatabase.DeleteAsset(parameters.AssetPath);

            return new AssetDeleteCommandResult
            {
                AssetPath = parameters.AssetPath,
                Deleted = deleted,
            };
        }
    }
}
