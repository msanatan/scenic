using NUnit.Framework;
using System.IO;
using UniBridge.Editor;
using UniBridge.Editor.Commands.Recovery;

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
                    ParamsJson = "{\"ids\":[\"cmd-recover\"]}",
                };

                var response = CommandRouter.Route(request, executeEnabled: true);
                Assert.IsTrue(response.Success);
                var result = response.Result as RecoverResultsCommandResult;
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Results);
                Assert.AreEqual(1, result.Results.Length);
                Assert.AreEqual("cmd-recover", result.Results[0].Id);
            }
            finally
            {
                Directory.Delete(stateDir, true);
            }
        }
    }
}
