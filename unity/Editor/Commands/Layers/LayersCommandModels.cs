using Newtonsoft.Json;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Commands.Layers
{
    public sealed class LayersRemoveCommandParams
    {
        public string Name;

        public static LayersRemoveCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var name = CommandModelHelpers.ReadOptionalString(payload, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            return new LayersRemoveCommandParams
            {
                Name = name.Trim(),
            };
        }
    }

    public sealed class LayersAddCommandParams
    {
        public string Name;

        public static LayersAddCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var name = CommandModelHelpers.ReadOptionalString(payload, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            return new LayersAddCommandParams
            {
                Name = name.Trim(),
            };
        }
    }

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

    public sealed class LayersAddCommandResult
    {
        [JsonProperty("layer")]
        public LayerItem Layer;

        [JsonProperty("added")]
        public bool Added;

        [JsonProperty("total")]
        public int Total;
    }

    public sealed class LayersRemoveCommandResult
    {
        [JsonProperty("layer")]
        public LayerItem Layer;

        [JsonProperty("removed")]
        public bool Removed;

        [JsonProperty("total")]
        public int Total;
    }
}
