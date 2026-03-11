using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    internal static class AssetPathHelpers
    {
        public static string RequireAssetPath(string assetPath, string paramName = "assetPath")
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                throw new CommandHandlingException($"params.{paramName} is required.");
            }

            return assetPath.Trim().Replace('\\', '/');
        }

        public static string RequireWritableAssetPath(string assetPath, string paramName = "assetPath")
        {
            var path = RequireAssetPath(assetPath, paramName);
            if (!path.StartsWith("Assets/", System.StringComparison.Ordinal))
            {
                throw new CommandHandlingException(
                    $"params.{paramName} must be a project asset path starting with 'Assets/'.");
            }

            return path;
        }

        public static string RequireExistingAsset(string assetPath, string paramName = "assetPath")
        {
            var path = RequireAssetPath(assetPath, paramName);
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                throw new CommandHandlingException($"Asset not found at path: {path}");
            }

            return path;
        }

        public static string RequireExistingWritableAsset(string assetPath, string paramName = "assetPath")
        {
            var path = RequireWritableAssetPath(assetPath, paramName);
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                throw new CommandHandlingException($"Asset not found at path: {path}");
            }

            return path;
        }

        public static string GetAssetTypeName(string assetPath)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            return asset != null ? asset.GetType().Name : "Unknown";
        }

        public static string GetAssetName(string assetPath)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            return asset != null ? asset.name : System.IO.Path.GetFileNameWithoutExtension(assetPath);
        }

        public static void EnsureParentFolder(string assetPath)
        {
            var dir = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (dir == null || AssetDatabase.IsValidFolder(dir)) return;

            EnsureParentFolder(dir);
            var parent = System.IO.Path.GetDirectoryName(dir)?.Replace('\\', '/') ?? "Assets";
            var folderName = System.IO.Path.GetFileName(dir);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
