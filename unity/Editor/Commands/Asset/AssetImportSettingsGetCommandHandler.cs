using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.importSettings.get")]
    public sealed class AssetImportSettingsGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetImportSettingsGetCommandParams.From(request);
            var importer = AssetImportSettingsHelpers.GetImporterOrThrow(parameters.AssetPath);
            var serialized = new SerializedObject(importer);
            var properties = AssetImportSettingsHelpers.ReadProperties(serialized, parameters.Properties);

            return new AssetImportSettingsGetCommandResult
            {
                AssetPath = parameters.AssetPath,
                ImporterType = importer.GetType().Name,
                Properties = properties,
            };
        }
    }
}
