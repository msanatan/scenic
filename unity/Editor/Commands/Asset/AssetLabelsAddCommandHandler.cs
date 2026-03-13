using System;
using System.Collections.Generic;
using UnityEditor;

namespace Scenic.Editor.Commands.Asset
{
    [ScenicCommand("asset.labels.add")]
    public sealed class AssetLabelsAddCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = AssetLabelsAddCommandParams.From(request);
            var asset = AssetDatabase.LoadMainAssetAtPath(parameters.AssetPath);
            var existing = new HashSet<string>(AssetDatabase.GetLabels(asset), StringComparer.Ordinal);
            var added = new List<string>();

            foreach (var label in parameters.Labels)
            {
                if (!string.IsNullOrWhiteSpace(label) && existing.Add(label.Trim()))
                {
                    added.Add(label.Trim());
                }
            }

            if (added.Count > 0)
            {
                var allLabels = new string[existing.Count];
                existing.CopyTo(allLabels);
                AssetDatabase.SetLabels(asset, allLabels);
                AssetDatabase.SaveAssets();
            }

            var currentLabels = AssetDatabase.GetLabels(asset);
            return new AssetLabelsAddCommandResult
            {
                AssetPath = parameters.AssetPath,
                Labels = currentLabels,
                Added = added.ToArray(),
            };
        }
    }
}
