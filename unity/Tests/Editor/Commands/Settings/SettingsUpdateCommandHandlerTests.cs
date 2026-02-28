using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Settings;
using Scenic.Editor.Settings;

namespace Scenic.Editor.Tests.Commands.Settings
{
    [TestFixture]
    public class SettingsUpdateCommandHandlerTests
    {
        private string _testDir;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "scenic-settings-update-test-" + Path.GetRandomFileName());
            Directory.CreateDirectory(_testDir);

            var service = new SettingsService();
            service.Initialize(_testDir);
            SettingsRuntime.SetService(service);
        }

        [TearDown]
        public void TearDown()
        {
            SettingsRuntime.ClearService();
            if (Directory.Exists(_testDir))
            {
                Directory.Delete(_testDir, true);
            }
        }

        [Test]
        public void Route_SettingsUpdate_SuccessPath()
        {
            var response = CommandRouter.Route(new CommandRequest
            {
                Id = "settings-update-1",
                Command = "settings.update",
                ParamsJson = "{\"executeEnabled\":true}",
            }, executeEnabled: false);

            Assert.IsTrue(response.Success);
            var result = response.Result as SettingsCommandResult;
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ExecuteEnabled);
            Assert.IsTrue(StateManager.ReadExecuteEnabled(_testDir));
        }

        [Test]
        public void Route_SettingsUpdate_ValidationFailure()
        {
            var response = CommandRouter.Route(new CommandRequest
            {
                Id = "settings-update-err",
                Command = "settings.update",
                ParamsJson = "{}",
            }, executeEnabled: false);

            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
            StringAssert.Contains("executeenabled", response.Error.ToLower());
        }

        [Test]
        public void Route_SettingsUpdate_PersistenceFailure_RollsBackMemory()
        {
            Directory.CreateDirectory(Path.Combine(_testDir, "config.json.tmp"));

            var failure = CommandRouter.Route(new CommandRequest
            {
                Id = "settings-update-fail",
                Command = "settings.update",
                ParamsJson = "{\"executeEnabled\":true}",
            }, executeEnabled: false);

            Assert.IsFalse(failure.Success);
            Assert.IsNotNull(failure.Error);
            StringAssert.Contains("persist", failure.Error.ToLower());

            var current = CommandRouter.Route(new CommandRequest
            {
                Id = "settings-get-after-fail",
                Command = "settings.get",
                ParamsJson = "{}",
            }, executeEnabled: false);

            Assert.IsTrue(current.Success);
            var result = current.Result as SettingsCommandResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ExecuteEnabled);
        }
    }
}
