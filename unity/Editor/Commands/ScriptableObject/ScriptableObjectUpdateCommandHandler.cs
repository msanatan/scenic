using System.Collections.Generic;
using Scenic.Editor.Commands;
using UnityEditor;

namespace Scenic.Editor.Commands.ScriptableObjects
{
    [ScenicCommand("scriptableobject.update")]
    public sealed class ScriptableObjectUpdateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ScriptableObjectUpdateCommandParams.From(request);
            var assetPath = ScriptableObjectAssetHelpers.NormalizeAssetPath(parameters.AssetPath, requireExists: true);
            var asset = ScriptableObjectAssetHelpers.LoadRequired(assetPath);

            var appliedFields = new List<string>();
            var ignoredFields = new List<string>();
            ScriptableObjectValueApplier.Apply(
                asset,
                parameters.Values,
                parameters.Strict,
                appliedFields,
                ignoredFields,
                "values");

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();

            return new ScriptableObjectUpdateCommandResult
            {
                Asset = ScriptableObjectAssetHelpers.BuildSummary(asset, assetPath),
                AppliedFields = appliedFields.ToArray(),
                IgnoredFields = ignoredFields.ToArray(),
            };
        }
    }
}
