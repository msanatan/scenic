using UnityEditor;

namespace UniBridge.Editor.Commands.Domain
{
    [UniBridgeCommand("domain.reload")]
    public sealed class DomainReloadCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            AssetDatabase.Refresh();

            return new DomainReloadCommandResult
            {
                Triggered = true,
            };
        }
    }
}
