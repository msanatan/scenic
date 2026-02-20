using System.Collections.Generic;
using UnityEngine;

namespace UniBridge.Editor.Commands.Layers
{
    [UniBridgeCommand("layers.get")]
    public sealed class LayersGetCommandHandler : ICommandHandler
    {
        private const int LayerSlotCount = 32;

        public object Handle(CommandRequest request)
        {
            var parameters = LayersGetCommandParams.From(request);
            var all = BuildLayerItems();
            var page = Pagination.Slice(all, parameters.Paging, out var total);

            return new LayersGetCommandResult
            {
                Layers = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }

        private static List<LayerItem> BuildLayerItems()
        {
            var items = new List<LayerItem>(LayerSlotCount);
            for (var i = 0; i < LayerSlotCount; i++)
            {
                var name = LayerMask.LayerToName(i) ?? string.Empty;
                items.Add(new LayerItem
                {
                    Index = i,
                    Name = name,
                    IsBuiltIn = i < 8,
                    IsUserEditable = i >= 8,
                    IsOccupied = !string.IsNullOrWhiteSpace(name),
                });
            }

            return items;
        }
    }
}
