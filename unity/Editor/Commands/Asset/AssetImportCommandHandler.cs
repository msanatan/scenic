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

            var validNames = Enum.GetNames(typeof(ImportAssetOptions));
            var tokens = options.Split(',');

            foreach (var token in tokens)
            {
                var trimmed = token.Trim();
                var found = false;
                foreach (var name in validNames)
                {
                    if (string.Equals(trimmed, name, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new CommandHandlingException(
                        $"Invalid import options value: {options}. " +
                        $"Valid values: {string.Join(", ", validNames)}.");
                }
            }

            Enum.TryParse<ImportAssetOptions>(options, true, out var parsed);
            return parsed;
        }
    }
}
