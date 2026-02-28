using Scenic.Editor.Settings;

namespace Scenic.Editor.Commands.Settings
{
    [ScenicCommand("settings.get")]
    public sealed class SettingsGetCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var settings = SettingsRuntime.GetRequiredService().Get();
            return SettingsCommandResult.FromModel(settings);
        }
    }
}
