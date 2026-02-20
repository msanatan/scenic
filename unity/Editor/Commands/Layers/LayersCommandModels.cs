using Newtonsoft.Json;
using UniBridge.Editor.Commands;

namespace UniBridge.Editor.Commands.Layers
{
    public sealed class LayersGetCommandParams
    {
        public PaginationParams Paging;

        public static LayersGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            return new LayersGetCommandParams
            {
                Paging = PaginationParams.From(payload, defaultLimit: 50, defaultOffset: 0),
            };
        }
    }

    public sealed class LayerItem
    {
        [JsonProperty("index")]
        public int Index;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("isBuiltIn")]
        public bool IsBuiltIn;

        [JsonProperty("isUserEditable")]
        public bool IsUserEditable;

        [JsonProperty("isOccupied")]
        public bool IsOccupied;
    }

    public sealed class LayersGetCommandResult
    {
        [JsonProperty("layers")]
        public LayerItem[] Layers;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }
}
