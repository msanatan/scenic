using Newtonsoft.Json;

namespace Scenic.Editor.Commands.Domain
{
    public sealed class DomainReloadCommandResult
    {
        [JsonProperty("triggered")]
        public bool Triggered;
    }
}
