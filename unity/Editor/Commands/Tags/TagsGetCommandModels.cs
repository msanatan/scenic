using Newtonsoft.Json;

namespace UniBridge.Editor.Commands.Tags
{
    public sealed class TagsGetCommandResult
    {
        [JsonProperty("tags")]
        public TagItem[] Tags;

        [JsonProperty("total")]
        public int Total;
    }

    public sealed class TagItem
    {
        [JsonProperty("name")]
        public string Name;

        [JsonProperty("isBuiltIn")]
        public bool IsBuiltIn;
    }
}
