namespace Scenic.Editor.Commands
{
    public interface ICommandHandler
    {
        object Handle(CommandRequest request);
    }
}
