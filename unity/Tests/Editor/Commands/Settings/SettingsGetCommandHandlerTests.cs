using System.IO;
using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Settings;
using Scenic.Editor.Settings;

namespace Scenic.Editor.Tests.Commands.Settings
{
    [TestFixture]
    public class SettingsGetCommandHandlerTests
    {
        private string _testDir;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "scenic-settings-get-test-" + Path.GetRandomFileName());
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
        public void Route_SettingsGet_ReturnsCurrentSettings()
        {
            var response = CommandRouter.Route(new CommandRequest
            {
                Id = "settings-get-1",
                Command = "settings.get",
                ParamsJson = "{}",
            }, executeEnabled: false);

            Assert.IsTrue(response.Success);
            var result = response.Result as SettingsCommandResult;
            Assert.IsNotNull(result);
            Assert.IsFalse(result.ExecuteEnabled);
        }
    }
}
