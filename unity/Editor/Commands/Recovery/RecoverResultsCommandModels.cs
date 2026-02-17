namespace UniBridge.Editor.Commands.Recovery
{
    public sealed class RecoverResultsCommandParams
    {
        public string[] Ids;

        public static RecoverResultsCommandParams From(CommandRequest request)
        {
            return new RecoverResultsCommandParams
            {
                Ids = request == null ? null : request.GetStringArrayParam("ids"),
            };
        }
    }
}
