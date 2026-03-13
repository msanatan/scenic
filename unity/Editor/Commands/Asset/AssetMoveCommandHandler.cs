using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.move")]
    public sealed class AssetMoveCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetMoveCommandParams.From(request);
            var guid = AssetDatabase.AssetPathToGUID(parameters.AssetPath);
            AssetPathHelpers.EnsureParentFolder(parameters.NewPath);
            var error = AssetDatabase.MoveAsset(parameters.AssetPath, parameters.NewPath);

            if (!string.IsNullOrEmpty(error))
            {
                throw new CommandHandlingException($"Failed to move asset: {error}");
            }

            return new AssetMoveCommandResult
            {
                OldPath = parameters.AssetPath,
                NewPath = parameters.NewPath,
                Guid = guid,
            };
        }
    }
}
