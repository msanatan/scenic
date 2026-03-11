using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.importSettings.set", RequiresExecuteEnabled = true)]
    public sealed class AssetImportSettingsSetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetImportSettingsSetCommandParams.From(request);
            var importer = AssetImportSettingsHelpers.GetImporterOrThrow(parameters.AssetPath);
            var serialized = new SerializedObject(importer);
            var applied = AssetImportSettingsHelpers.ApplyProperties(serialized, parameters.Properties);

            importer.SaveAndReimport();

            return new AssetImportSettingsSetCommandResult
            {
                AssetPath = parameters.AssetPath,
                ImporterType = importer.GetType().Name,
                AppliedProperties = applied,
            };
        }
    }
}
