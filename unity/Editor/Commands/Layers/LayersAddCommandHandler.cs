using System;
using UnityEditor;
using UnityEngine;

namespace UniBridge.Editor.Commands.Layers
{
    [UniBridgeCommand("layers.add")]
    public sealed class LayersAddCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = LayersAddCommandParams.From(request);
            ValidateName(parameters.Name);

            var existingIndex = LayerDefinitions.FindLayerIndexByName(parameters.Name);
            if (existingIndex >= 0)
            {
                return new LayersAddCommandResult
                {
                    Layer = BuildLayerItem(existingIndex),
                    Added = false,
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

            var targetIndex = FindFirstEmptyUserLayerIndex(layersProperty);
            if (targetIndex < 0)
            {
                throw new CommandHandlingException("No available user layer slots in range 8-31.");
            }

            var item = layersProperty.GetArrayElementAtIndex(targetIndex);
            item.stringValue = parameters.Name;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();

            return new LayersAddCommandResult
            {
                Layer = BuildLayerItem(targetIndex),
                Added = true,
                Total = LayerDefinitions.LayerSlotCount,
            };
        }

        private static LayerItem BuildLayerItem(int index)
        {
            var name = LayerMask.LayerToName(index) ?? string.Empty;
            return new LayerItem
            {
                Index = index,
                Name = name,
                IsBuiltIn = LayerDefinitions.IsBuiltIn(index),
                IsUserEditable = LayerDefinitions.IsUserEditable(index),
                IsOccupied = !string.IsNullOrWhiteSpace(name),
            };
        }

        private static int FindFirstEmptyUserLayerIndex(SerializedProperty layersProperty)
        {
            for (var i = LayerDefinitions.FirstUserLayerIndex; i <= LayerDefinitions.LastUserLayerIndex; i++)
            {
                var item = layersProperty.GetArrayElementAtIndex(i);
                if (string.IsNullOrWhiteSpace(item.stringValue))
                {
                    return i;
                }
            }

            return -1;
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            if (name.IndexOf(',') >= 0)
            {
                throw new CommandHandlingException("params.name cannot contain ','.");
            }

            if (name.Length > 64)
            {
                throw new CommandHandlingException("params.name must be 64 characters or fewer.");
            }
        }
    }
}
