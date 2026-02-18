namespace UniBridge.Editor.Commands.Logs
{
    [UniBridgeCommand("logs")]
    public sealed class LogsCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = LogsCommandParams.From(request);
            var page = LogBuffer.Query(parameters.Severity, parameters.Limit, parameters.Offset, out var total);

            return new LogsCommandResult
            {
                Logs = page,
                Total = total,
                Limit = parameters.Limit,
                Offset = parameters.Offset,
            };
        }
    }
}
