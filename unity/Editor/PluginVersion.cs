using UnityEditor.PackageManager;

namespace Scenic.Editor
{
    public static class PluginVersion
    {
        private static string _cached;

        public static string Get()
        {
            if (_cached != null) return _cached;

            var info = PackageInfo.FindForAssetPath("Packages/com.msanatan.scenic/package.json");
            _cached = info?.version ?? "0.0.0";
            return _cached;
        }
    }
}
