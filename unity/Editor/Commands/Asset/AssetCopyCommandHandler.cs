using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.copy")]
    public sealed class AssetCopyCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetCopyCommandParams.From(request);
            AssetPathHelpers.EnsureParentFolder(parameters.NewPath);
            var success = AssetDatabase.CopyAsset(parameters.AssetPath, parameters.NewPath);

            if (!success)
            {
                throw new CommandHandlingException(
                    $"Failed to copy asset from '{parameters.AssetPath}' to '{parameters.NewPath}'.");
            }

            var newGuid = AssetDatabase.AssetPathToGUID(parameters.NewPath);
            return new AssetCopyCommandResult
            {
                SourcePath = parameters.AssetPath,
                NewPath = parameters.NewPath,
                Guid = newGuid,
            };
        }
    }
}
