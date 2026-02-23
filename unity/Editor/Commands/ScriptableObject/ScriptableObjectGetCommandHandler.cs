using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.ScriptableObjects
{
    [ScenicCommand("scriptableobject.get")]
    public sealed class ScriptableObjectGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ScriptableObjectGetCommandParams.From(request);
            var assetPath = ScriptableObjectAssetHelpers.NormalizeAssetPath(parameters.AssetPath, requireExists: true);
            var asset = ScriptableObjectAssetHelpers.LoadRequired(assetPath);

            return new ScriptableObjectGetCommandResult
            {
                Asset = ScriptableObjectAssetHelpers.BuildSummary(asset, assetPath),
                Serialized = ScriptableObjectAssetHelpers.ReadSerialized(asset),
            };
        }
    }
}
