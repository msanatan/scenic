using System;

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
                return CommandResponse.Ok(request.Id, new { recovered = true });
            }

            return CommandResponse.Fail(request.Id, $"Unknown command: {request.Command}");
        }
    }
}
