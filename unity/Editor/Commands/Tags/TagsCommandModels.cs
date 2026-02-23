using System;
using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Tags
{
    public sealed class TagsGetCommandParams
    {
        public PaginationParams Paging;

        public static TagsGetCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            return new TagsGetCommandParams
            {
                Paging = PaginationParams.From(payload, defaultLimit: 50, defaultOffset: 0),
            };
        }
    }

    public sealed class TagsAddCommandParams
    {
        public string Name;

        public static TagsAddCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var name = CommandModelHelpers.ReadOptionalString(payload, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            return new TagsAddCommandParams
            {
                Name = name.Trim(),
            };
        }
    }

    public sealed class TagsRemoveCommandParams
    {
        public string Name;

        public static TagsRemoveCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var name = CommandModelHelpers.ReadOptionalString(payload, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new CommandHandlingException("params.name is required.");
            }

            return new TagsRemoveCommandParams
            {
                Name = name.Trim(),
            };
        }
    }

    public sealed class TagsGetCommandResult
    {
        [JsonProperty("tags")]
        public TagItem[] Tags;

        [JsonProperty("total")]
        public int Total;

        [JsonProperty("limit")]
        public int Limit;

        [JsonProperty("offset")]
        public int Offset;
    }

    public sealed class TagItem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("isBuiltIn")]
        public bool IsBuiltIn;
    }

    public sealed class TagsAddCommandResult
    {
        [JsonProperty("tag")]
        public TagItem Tag;

        [JsonProperty("added")]
        public bool Added;

        [JsonProperty("total")]
        public int Total;
    }

    public sealed class TagsRemoveCommandResult
    {
        [JsonProperty("tag")]
        public TagItem Tag;

        [JsonProperty("removed")]
        public bool Removed;

        [JsonProperty("total")]
        public int Total;
    }
}
