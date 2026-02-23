namespace Scenic.Editor.Commands.Execute
{
    public sealed class ExecuteCommandParams
    {
        public string Code;

        public static ExecuteCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);

            return new ExecuteCommandParams
            {
                Code = CommandModelHelpers.ReadOptionalString(payload, "code"),
            };
        }
    }
}
