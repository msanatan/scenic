namespace UniBridge.Editor.Commands.Execute
{
    public sealed class ExecuteCommandParams
    {
        public string Code;

        public static ExecuteCommandParams From(CommandRequest request)
        {
            return new ExecuteCommandParams
            {
                Code = request == null ? null : request.GetStringParam("code"),
            };
        }
    }
}
