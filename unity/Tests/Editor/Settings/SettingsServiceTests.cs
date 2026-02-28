using System.IO;
using NUnit.Framework;
using Scenic.Editor.Commands;
using Scenic.Editor.Settings;

namespace Scenic.Editor.Tests.Settings
{
    [TestFixture]
    public class SettingsServiceTests
    {
        private string _testDir;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "scenic-settings-service-test-" + Path.GetRandomFileName());
            Directory.CreateDirectory(_testDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Test]
        public void Initialize_LoadsPersistedValues()
        {
            StateManager.WriteSettings(_testDir, new ScenicSettingsModel
            {
                ExecuteEnabled = true,
            });

            var service = new SettingsService();
            service.Initialize(_testDir);

            var settings = service.Get();
            Assert.IsTrue(settings.ExecuteEnabled);
        }

        [Test]
        public void Update_Success_UpdatesDiskAndMemory()
        {
            var service = new SettingsService();
            service.Initialize(_testDir);

            var updated = service.Update(new ScenicSettingsPatch
            {
                ExecuteEnabled = true,
            });

            Assert.IsTrue(updated.ExecuteEnabled);
            Assert.IsTrue(service.Get().ExecuteEnabled);
            Assert.IsTrue(StateManager.ReadExecuteEnabled(_testDir));
        }

        [Test]
        public void Update_PersistenceFailure_DoesNotMutateMemory()
        {
            var service = new SettingsService();
            service.Initialize(_testDir);

            var configPath = Path.Combine(_testDir, "config.json");
            var directoryPath = Path.Combine(_testDir, "config.json.tmp");
            Directory.CreateDirectory(directoryPath);

            Assert.Throws<CommandHandlingException>(() =>
                service.Update(new ScenicSettingsPatch
                {
                    ExecuteEnabled = true,
                }));

            Assert.IsFalse(service.Get().ExecuteEnabled);
            Assert.IsTrue(Directory.Exists(directoryPath));
            Assert.IsFalse(File.Exists(configPath));
        }

        [Test]
        public void Update_InvalidPayload_Rejects()
        {
            var service = new SettingsService();
            service.Initialize(_testDir);

            Assert.Throws<CommandHandlingException>(() =>
                service.Update(new ScenicSettingsPatch()));
            Assert.IsFalse(service.Get().ExecuteEnabled);
        }
    }
}
