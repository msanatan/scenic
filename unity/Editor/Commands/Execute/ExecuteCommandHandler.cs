namespace UniBridge.Editor.Commands.Execute
{
    [UniBridgeCommand("execute", RequiresExecuteEnabled = true)]
    public sealed class ExecuteCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = ExecuteCommandParams.From(request);
            if (string.IsNullOrWhiteSpace(parameters.Code))
            {
                throw new CommandHandlingException("Missing params.code for execute command.");
            }

            return CSharpExecutor.Evaluate(parameters.Code);
        }
    }
}
