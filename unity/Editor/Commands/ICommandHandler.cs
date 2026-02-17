namespace UniBridge.Editor.Commands
{
    public interface ICommandHandler
    {
        CommandResponse Handle(CommandRequest request);
    }
}
