using Scenic.Editor.Settings;

namespace Scenic.Editor.Commands.Settings
{
    [ScenicCommand("settings.update")]
    public sealed class SettingsUpdateCommandHandler : ICommandHandler
    {
        public object Handle(CommandRequest request)
        {
            var parameters = SettingsUpdateCommandParams.From(request);
            var settings = SettingsRuntime.GetRequiredService().Update(parameters.ToPatch());
            return SettingsCommandResult.FromModel(settings);
        }
    }
}
