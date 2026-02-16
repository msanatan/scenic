using System;
using System.Text;

namespace UniBridge.Editor
{
    public static class CommandRouter
    {
        public static CommandResponse Route(CommandRequest request)
        {
            if (request == null)
            {
                return CommandResponse.Fail(string.Empty, "Request is null.");
            }

            if (string.Equals(request.Command, "execute", StringComparison.OrdinalIgnoreCase))
            {
                var code = request.Params == null ? null : request.Params.Code;
                if (string.IsNullOrWhiteSpace(code))
                {
                    return CommandResponse.Fail(request.Id, "Missing params.code for execute command.");
                }

                return CSharpExecutor.Execute(request.Id, code);
            }

            if (string.Equals(request.Command, "recoverResults", StringComparison.OrdinalIgnoreCase))
            {
                var ids = request.Params == null || request.Params.Ids == null
                    ? Array.Empty<string>()
                    : request.Params.Ids;
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
                return CommandResponse.Ok(request.Id, recovered.ToString());
            }

            return CommandResponse.Fail(request.Id, $"Unknown command: {request.Command}");
        }
    }
}
