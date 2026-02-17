using System;
using System.Text;

namespace UniBridge.Editor.Commands.Recovery
{
    [UniBridgeCommand("recoverResults")]
    public sealed class RecoverResultsCommandHandler : ICommandHandler
    {
        public CommandResponse Handle(CommandRequest request)
        {
            var parameters = RecoverResultsCommandParams.From(request);
            var ids = parameters.Ids ?? Array.Empty<string>();
            var stateDirectory = StateManager.CurrentStateDirectory();
            var recovered = new StringBuilder();
            recovered.Append("{\"results\":[");
            var first = true;

            for (var i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                if (string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                var result = StateManager.ReadResult(stateDirectory, id);
                if (result == null)
                {
                    continue;
                }

                if (!first)
                {
                    recovered.Append(',');
                }

                recovered.Append(result.ToJson());
                first = false;
            }

            recovered.Append("]}");
            return CommandResponse.Ok(request == null ? string.Empty : request.Id, recovered.ToString());
        }
    }
}
