using System;
using Scenic.Editor.Commands;

namespace Scenic.Editor.Settings
{
    public sealed class SettingsService
    {
        private string _stateDirectory;
        private ScenicSettingsModel _effectiveSettings = ScenicSettingsModel.Default();

        public void Initialize(string hashOrDirectory)
        {
            _stateDirectory = StateManager.ResolveStateDirectory(hashOrDirectory);
            StateManager.EnsureStateDirectory(_stateDirectory);
            _effectiveSettings = StateManager.ReadSettingsOrDefault(_stateDirectory);
        }

        public ScenicSettingsModel Get()
        {
            EnsureInitialized();
            return _effectiveSettings.Clone();
        }

        public ScenicSettingsModel Update(ScenicSettingsPatch patch)
        {
            EnsureInitialized();
            if (patch == null)
            {
                throw new CommandHandlingException("Settings patch is required.");
            }

            if (!patch.ExecuteEnabled.HasValue)
            {
                throw new CommandHandlingException("params.executeEnabled is required.");
            }

            var next = _effectiveSettings.Clone();
            next.ExecuteEnabled = patch.ExecuteEnabled.Value;

            try
            {
                StateManager.WriteSettings(_stateDirectory, next);
            }
            catch (Exception ex)
            {
                throw new CommandHandlingException($"Failed to persist settings: {ex.Message}");
            }

            _effectiveSettings = next;
            return _effectiveSettings.Clone();
        }

        private void EnsureInitialized()
        {
            if (string.IsNullOrWhiteSpace(_stateDirectory))
            {
                throw new InvalidOperationException("SettingsService.Initialize must be called before use.");
            }
        }
    }
}
