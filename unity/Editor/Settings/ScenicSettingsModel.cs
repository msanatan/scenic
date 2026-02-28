using Newtonsoft.Json;

namespace Scenic.Editor.Settings
{
    public sealed class ScenicSettingsModel
    {
        [JsonProperty("executeEnabled")]
        public bool ExecuteEnabled;

        public static ScenicSettingsModel Default()
        {
            return new ScenicSettingsModel
            {
                ExecuteEnabled = false,
            };
        }

        public ScenicSettingsModel Clone()
        {
            return new ScenicSettingsModel
            {
                ExecuteEnabled = ExecuteEnabled,
            };
        }
    }

    public sealed class ScenicSettingsPatch
    {
        public bool? ExecuteEnabled;
    }
}
