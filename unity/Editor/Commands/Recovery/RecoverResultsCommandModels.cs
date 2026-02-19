using Newtonsoft.Json;

namespace UniBridge.Editor.Commands.Recovery
{
    public sealed class RecoverResultsCommandParams
    {
        public string[] Ids;

        public static RecoverResultsCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var ids = CommandModelHelpers.ReadOptionalStringArray(payload, "ids");

            return new RecoverResultsCommandParams
            {
                Ids = ids,
            };
        }
    }

    public sealed class RecoverResultsCommandResult
    {
        [JsonProperty("results")]
        public CommandResponse[] Results;
    }
}
