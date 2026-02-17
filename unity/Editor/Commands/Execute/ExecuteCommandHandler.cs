namespace UniBridge.Editor.Commands.Execute
{
    [UniBridgeCommand("execute", RequiresExecuteEnabled = true)]
    public sealed class ExecuteCommandHandler : ICommandHandler
    {
        public CommandResponse Handle(CommandRequest request)
        {
            var parameters = ExecuteCommandParams.From(request);
            if (string.IsNullOrWhiteSpace(parameters.Code))
            {
                return CommandResponse.Fail(request == null ? string.Empty : request.Id, "Missing params.code for execute command.");
            }

            return CSharpExecutor.Execute(request.Id, parameters.Code);
        }
    }
}
