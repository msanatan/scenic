using System;
using System.Collections.Generic;
using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.labels.remove", RequiresExecuteEnabled = true)]
    public sealed class AssetLabelsRemoveCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetLabelsRemoveCommandParams.From(request);
            var asset = AssetDatabase.LoadMainAssetAtPath(parameters.AssetPath);
            var existing = new List<string>(AssetDatabase.GetLabels(asset));
            var toRemove = new HashSet<string>(StringComparer.Ordinal);

            foreach (var label in parameters.Labels)
            {
                if (!string.IsNullOrWhiteSpace(label))
                {
                    toRemove.Add(label.Trim());
                }
            }

            var removed = new List<string>();
            for (var i = existing.Count - 1; i >= 0; i--)
            {
                if (toRemove.Contains(existing[i]))
                {
                    removed.Add(existing[i]);
                    existing.RemoveAt(i);
                }
            }

            if (removed.Count > 0)
            {
                AssetDatabase.SetLabels(asset, existing.ToArray());
                AssetDatabase.SaveAssets();
            }

            var currentLabels = AssetDatabase.GetLabels(asset);
            return new AssetLabelsRemoveCommandResult
            {
                AssetPath = parameters.AssetPath,
                Labels = currentLabels,
                Removed = removed.ToArray(),
            };
        }
    }
}
