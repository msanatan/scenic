using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Scenic.Editor.Commands.Settings
{
    public sealed class SettingsCommandResult
    {
        [JsonProperty("executeEnabled")]
        public bool ExecuteEnabled;

        public static SettingsCommandResult FromModel(Scenic.Editor.Settings.ScenicSettingsModel model)
        {
            return new SettingsCommandResult
            {
                ExecuteEnabled = model.ExecuteEnabled,
            };
        }
    }

    public sealed class SettingsUpdateCommandParams
    {
        public bool? ExecuteEnabled;

        public Scenic.Editor.Settings.ScenicSettingsPatch ToPatch()
        {
            return new Scenic.Editor.Settings.ScenicSettingsPatch
            {
                ExecuteEnabled = ExecuteEnabled,
            };
        }

        public static SettingsUpdateCommandParams From(CommandRequest request)
        {
            var payload = CommandModelHelpers.ParsePayload(request);
            var token = payload["executeEnabled"];

            if (token == null || token.Type == JTokenType.Null)
            {
                return new SettingsUpdateCommandParams();
            }

            if (token.Type != JTokenType.Boolean)
            {
                throw new CommandHandlingException("params.executeEnabled must be a boolean.");
            }

            return new SettingsUpdateCommandParams
            {
                ExecuteEnabled = token.Value<bool>(),
            };
        }
    }
}
