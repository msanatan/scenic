using System;
using UnityEditor;

namespace Scenic.Editor.Commands.Layers
{
    [ScenicCommand("layers.remove")]
    public sealed class LayersRemoveCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = LayersRemoveCommandParams.From(request);
            var index = LayerDefinitions.FindLayerIndexByName(parameters.Name);

            if (index >= 0 && LayerDefinitions.IsBuiltIn(index))
            {
                throw new CommandHandlingException($"Cannot remove built-in layer: {parameters.Name}");
            }

            if (index < 0)
            {
                return new LayersRemoveCommandResult
                {
                    Layer = new LayerItem
                    {
                        Index = -1,
                        Name = parameters.Name,
                        IsBuiltIn = false,
                        IsUserEditable = true,
                        IsOccupied = false,
                    },
                    Removed = false,
                    Total = LayerDefinitions.LayerSlotCount,
                };
            }

            var asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (asset == null || asset.Length == 0 || asset[0] == null)
            {
                throw new CommandHandlingException("Unable to load ProjectSettings/TagManager.asset.");
            }

            var serialized = new SerializedObject(asset[0]);
            var layersProperty = serialized.FindProperty("layers");
            if (layersProperty == null || !layersProperty.isArray)
            {
                throw new CommandHandlingException("Unable to resolve layers property in TagManager.");
            }

            for (var i = LayerDefinitions.LastUserLayerIndex; i >= LayerDefinitions.FirstUserLayerIndex; i--)
            {
                var item = layersProperty.GetArrayElementAtIndex(i);
                if (string.Equals(item.stringValue, parameters.Name, StringComparison.Ordinal))
                {
                    item.stringValue = string.Empty;
                }
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            return new LayersRemoveCommandResult
            {
                Layer = new LayerItem
                {
                    Index = index,
                    Name = parameters.Name,
                    IsBuiltIn = false,
                    IsUserEditable = true,
                    IsOccupied = false,
                },
                Removed = true,
                Total = LayerDefinitions.LayerSlotCount,
            };
        }
    }
}
