using NUnit.Framework;
using UniBridge.Editor;

namespace UniBridge.Tests.Editor
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
                Params = new CommandParams(),
            };

            var response = CommandRouter.Route(request);
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
                Params = new CommandParams(),
            };

            var response = CommandRouter.Route(request);
            Assert.IsFalse(response.Success);
            Assert.IsNotNull(response.Error);
        }
    }
}
