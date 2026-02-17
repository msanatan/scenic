using UniBridge.Editor.Commands;

namespace UniBridge.Editor
{
    public static class CommandRouter
    {
        public static CommandResponse Route(CommandRequest request, bool executeEnabled)
        {
            if (request == null)
            {
                return CommandResponse.Fail(string.Empty, "Request is null.");
            }

            if (!CommandRegistry.TryResolve(request.Command, out var registration))
            {
                return CommandResponse.Fail(request.Id, $"Unknown command: {request.Command}");
            }

            if (registration.RequiresExecuteEnabled && !executeEnabled)
            {
                return CommandResponse.Fail(request.Id, "Execute is disabled by plugin configuration.");
            }

            return registration.Handler.Handle(request);
        }
    }
}
