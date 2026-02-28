using System;

namespace Scenic.Editor.Settings
{
    public static class SettingsRuntime
    {
        private static SettingsService _service;

        public static void SetService(SettingsService service)
        {
            _service = service;
        }

        public static void ClearService()
        {
            _service = null;
        }

        public static SettingsService GetRequiredService()
        {
            if (_service == null)
            {
                throw new InvalidOperationException("Settings service is not initialized.");
            }

            return _service;
        }
    }
}
