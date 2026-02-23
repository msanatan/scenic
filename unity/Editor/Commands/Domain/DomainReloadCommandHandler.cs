using UnityEditor;

namespace Scenic.Editor.Commands.Domain
{
    [ScenicCommand("domain.reload")]
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
