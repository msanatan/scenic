using NUnit.Framework;
using UniBridge.Editor;

namespace UniBridge.Editor.Tests
{
    [TestFixture]
    public class CommandRouterTests
    {
        [Test]
        public void Route_UnknownCommand_ReturnsError()
        {
            var request = new CommandRequest
            {
                Id = "cmd-unknown",
                Command = "bad",
                ParamsJson = "{}",
            };

            var response = CommandRouter.Route(request, executeEnabled: true);
            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
        }

        [Test]
        public void Route_ExecuteWithoutCode_ReturnsError()
        {
            var request = new CommandRequest
            {
                Id = "cmd-missing-code",
                Command = "execute",
                ParamsJson = "{}",
            };

            var response = CommandRouter.Route(request, executeEnabled: true);
            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
        }

    }
}
