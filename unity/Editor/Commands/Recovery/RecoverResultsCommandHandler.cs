using System;
using System.Collections.Generic;

namespace UniBridge.Editor.Commands.Recovery
{
    [UniBridgeCommand("recoverResults")]
    public sealed class RecoverResultsCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = RecoverResultsCommandParams.From(request);
            var ids = parameters.Ids ?? Array.Empty<string>();
            var stateDirectory = StateManager.CurrentStateDirectory();
            var recovered = new List<CommandResponse>(ids.Length);

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

                recovered.Add(result);
            }

            return new RecoverResultsCommandResult { Results = recovered.ToArray() };
        }
    }
}
