namespace Scenic.Editor.Commands.Logs
{
    [ScenicCommand("logs")]
    public sealed class LogsCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = LogsCommandParams.From(request);
            var page = LogBuffer.Query(parameters.Severity, parameters.Paging, out var total);

            return new LogsCommandResult
            {
                Logs = page,
                Total = total,
                Limit = parameters.Paging.Limit,
                Offset = parameters.Paging.Offset,
            };
        }
    }
}
