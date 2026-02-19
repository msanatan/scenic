using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniBridge.Editor.Commands.Recovery
{
    public sealed class RecoverResultsCommandParams
    {
        public string[] Ids;

        public static RecoverResultsCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var idsToken = payload["ids"];
            if (idsToken != null && idsToken.Type != JTokenType.Null && !(idsToken is JArray))
            {
                throw new CommandHandlingException("params.ids must be an array.");
            }

            var idsArray = idsToken as JArray;
            var ids = idsArray == null ? null : new string[idsArray.Count];
            if (ids != null)
            {
                for (var i = 0; i < idsArray.Count; i++)
                {
                    var item = idsArray[i];
                    ids[i] = item == null || item.Type == JTokenType.Null
                        ? string.Empty
                        : item.Type == JTokenType.String
                            ? item.Value<string>()
                            : item.ToString(Newtonsoft.Json.Formatting.None);
                }
            }

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
