using System;
using System.IO;
using UnityEditor;
using UniBridge.Editor.Commands.GameObject;

namespace UniBridge.Editor.Commands.Prefab
{
    [UniBridgeCommand("prefab.save")]
    public sealed class PrefabSaveCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = PrefabSaveCommandParams.From(request);
            var source = GameObjectLookup.ResolveRequired(parameters.Path, parameters.InstanceId, "Target");
            var prefabPath = NormalizePrefabPath(parameters.PrefabPath);

            var parentDirectory = Path.GetDirectoryName(prefabPath);
            if (!string.IsNullOrWhiteSpace(parentDirectory) && !Directory.Exists(parentDirectory))
            {
                throw new CommandHandlingException($"Directory does not exist: {parentDirectory}");
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(source, prefabPath, out var success);
            if (!success || prefab == null)
            {
                throw new CommandHandlingException($"Failed to save prefab: {prefabPath}");
            }

            AssetDatabase.SaveAssets();

            return new PrefabSaveCommandResult
            {
                PrefabPath = prefabPath,
                SourceName = source.name,
                SourcePath = GameObjectLookup.BuildPath(source.transform),
                SourceInstanceId = source.GetInstanceID(),
            };
        }

        private static string NormalizePrefabPath(string path)
        {
            var normalized = string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/').Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                throw new CommandHandlingException("Missing required parameter: prefabPath");
            }

            if (Path.IsPathRooted(normalized))
            {
                throw new CommandHandlingException("Absolute paths are not supported. Use a project-relative prefab path.");
            }

            if (!normalized.StartsWith("Assets/", StringComparison.Ordinal))
            {
                throw new CommandHandlingException("prefabPath must start with 'Assets/'.");
            }

            if (!normalized.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            {
                throw new CommandHandlingException("prefabPath must end with '.prefab'.");
            }

            return normalized;
        }
    }
}
