using System;
using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.import")]
    public sealed class AssetImportCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetImportCommandParams.From(request);
            var importOptions = ParseImportOptions(parameters.Options);

            AssetDatabase.ImportAsset(parameters.AssetPath, importOptions);

            var importer = AssetImporter.GetAtPath(parameters.AssetPath);
            if (importer == null)
            {
                throw new CommandHandlingException(
                    $"Import did not produce a valid asset at path: {parameters.AssetPath}");
            }

            return new AssetImportCommandResult
            {
                AssetPath = parameters.AssetPath,
                ImporterType = importer.GetType().Name,
            };
        }

        private static ImportAssetOptions ParseImportOptions(string options)
        {
            if (string.IsNullOrEmpty(options))
            {
                return ImportAssetOptions.Default;
            }

            if (Enum.TryParse<ImportAssetOptions>(options, true, out var parsed))
            {
                return parsed;
            }

            throw new CommandHandlingException(
                $"Invalid import options value: {options}. " +
                "Valid values: Default, ForceUpdate, ForceSynchronousImport, ImportRecursive, DontDownloadFromCacheServer, ForceUncompressedImport.");
        }
    }
}
