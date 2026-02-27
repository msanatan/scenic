namespace Scenic.Editor.Commands.Material
{
    [ScenicCommand("material.get")]
    public sealed class MaterialGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = MaterialGetCommandParams.From(request);
            var assetPath = MaterialAssetHelpers.NormalizeAssetPath(parameters.AssetPath, requireExists: true);
            var material = MaterialAssetHelpers.LoadRequired(assetPath);

            return new MaterialGetCommandResult
            {
                Material = MaterialAssetHelpers.BuildSummary(material, assetPath),
            };
        }
    }
}
