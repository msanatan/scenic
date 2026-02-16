using NUnit.Framework;
using System.IO;
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

        [Test]
        public void Route_RecoverResults_ReturnsRequestedResultPayload()
        {
            var stateDir = Path.Combine(Path.GetTempPath(), "unibridge-router-test-" + Path.GetRandomFileName());
            Directory.CreateDirectory(stateDir);
            try
            {
                StateManager.WriteResult(stateDir, new CommandResponse
                {
                    Id = "cmd-recover",
                    Success = true,
                    Result = "ok",
                });

                StateManager.SetCurrentProjectHash(stateDir);

                var request = new CommandRequest
                {
                    Id = "recover-1",
                    Command = "recoverResults",
                    Params = new CommandParams { Ids = new[] { "cmd-recover" } },
                };

                var response = CommandRouter.Route(request);
                Assert.IsTrue(response.Success);
                Assert.That(response.Result, Does.Contain("cmd-recover"));
            }
            finally
            {
                Directory.Delete(stateDir, true);
            }
        }
    }
}
