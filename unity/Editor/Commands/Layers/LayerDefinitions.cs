using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniBridge.Editor.Commands.Layers
{
    internal static class LayerDefinitions
    {
        public const int LayerSlotCount = 32;
        public const int FirstUserLayerIndex = 8;
        public const int LastUserLayerIndex = 31;

        public static List<LayerItem> BuildLayerItems()
        {
            var items = new List<LayerItem>(LayerSlotCount);
            for (var i = 0; i < LayerSlotCount; i++)
            {
                var name = LayerMask.LayerToName(i) ?? string.Empty;
                items.Add(new LayerItem
                {
                    Index = i,
                    Name = name,
                    IsBuiltIn = IsBuiltIn(i),
                    IsUserEditable = IsUserEditable(i),
                    IsOccupied = !string.IsNullOrWhiteSpace(name),
                });
            }

            return items;
        }

        public static bool IsBuiltIn(int index)
        {
            return index >= 0 && index < FirstUserLayerIndex;
        }

        public static bool IsUserEditable(int index)
        {
            return index >= FirstUserLayerIndex && index <= LastUserLayerIndex;
        }

        public static int FindLayerIndexByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return -1;
            }

            for (var i = 0; i < LayerSlotCount; i++)
            {
                var current = LayerMask.LayerToName(i) ?? string.Empty;
                if (string.Equals(current, name, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
