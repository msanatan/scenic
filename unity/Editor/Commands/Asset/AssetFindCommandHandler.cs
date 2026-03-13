using System.Collections.Generic;
using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.find")]
    public sealed class AssetFindCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetFindCommandParams.From(request);
            var filter = BuildSearchFilter(parameters);
            var guids = AssetDatabase.FindAssets(filter);
            var items = new List<AssetFindItem>();

            for (var i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (string.IsNullOrEmpty(assetPath))
                {
                    continue;
                }

                var typeName = AssetPathHelpers.GetAssetTypeName(assetPath);

                // FindAssets type filter can return assets whose runtime type
                // differs from the search type (e.g. package assets). Post-filter
                // to keep the reported type consistent with the requested filter.
                if (!string.IsNullOrEmpty(parameters.Type) &&
                    !string.Equals(typeName, parameters.Type, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                items.Add(new AssetFindItem
                {
                    AssetPath = assetPath,
                    Guid = guids[i],
                    Type = typeName,
                    Name = AssetPathHelpers.GetAssetName(assetPath),
                });
            }

            var page = Pagination.Slice(items, parameters.Paging, out var total);
            return new AssetFindCommandResult
            {
                Assets = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }

        private static string BuildSearchFilter(AssetFindCommandParams parameters)
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(parameters.Query))
            {
                parts.Add(parameters.Query);
            }

            if (!string.IsNullOrEmpty(parameters.Type))
            {
                parts.Add($"t:{parameters.Type}");
            }

            if (parameters.Labels != null)
            {
                for (var i = 0; i < parameters.Labels.Length; i++)
                {
                    var label = parameters.Labels[i];
                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        parts.Add($"l:{label.Trim()}");
                    }
                }
            }

            return string.Join(" ", parts);
        }
    }
}
