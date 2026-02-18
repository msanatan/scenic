using NUnit.Framework;
using UniBridge.Editor;

namespace UniBridge.Editor.Tests.Commands.Test
{
    [TestFixture]
    public class TestRunCommandHandlerTests
    {
        [Test]
        public void Route_TestRun_PlayMode_ReturnsError()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "test-run-play",
                    Command = "test.run",
                    ParamsJson = "{\"mode\":\"play\",\"limit\":10,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            Assert.IsTrue(response.Error != null && response.Error.Contains("mode=edit"));
        }
    }
}
