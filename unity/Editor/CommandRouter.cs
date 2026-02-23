using Scenic.Editor.Commands;

namespace Scenic.Editor
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

            try
            {
                var result = registration.Handler.Handle(request);
                return CommandResponse.Ok(request.Id, result);
            }
            catch (CommandHandlingException ex)
            {
                return CommandResponse.Fail(request.Id, ex.Message);
            }
            catch (System.Exception ex)
            {
                return CommandResponse.Fail(request.Id, ex.Message);
            }
        }
    }
}
