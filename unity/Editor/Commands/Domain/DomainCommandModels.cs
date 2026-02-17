using Newtonsoft.Json;

namespace UniBridge.Editor.Commands.Domain
{
    public sealed class DomainReloadCommandResult
    {
        [JsonProperty("triggered")]
        public bool Triggered;
    }
}
