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

            var currentLabels = AssetDatabase.GetLabels(asset);
            var allLabels = new List<string>(currentLabels);
            var seen = new HashSet<string>(currentLabels, StringComparer.Ordinal);
            var added = new List<string>();

            foreach (var label in parameters.Labels)
            {
                var trimmed = label.Trim();
                if (trimmed.Length > 0 && seen.Add(trimmed))
                {
                    allLabels.Add(trimmed);
                    added.Add(trimmed);
                }
            }

            if (added.Count > 0)
            {
                AssetDatabase.SetLabels(asset, allLabels.ToArray());
                AssetDatabase.SaveAssets();
            }

            return new AssetLabelsAddCommandResult
            {
                AssetPath = parameters.AssetPath,
                Labels = AssetDatabase.GetLabels(asset),
                Added = added.ToArray(),
            };
        }
    }
}
