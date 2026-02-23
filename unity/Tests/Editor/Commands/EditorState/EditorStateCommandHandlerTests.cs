using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.EditorState;

namespace Scenic.Editor.Tests.Commands.EditorState
{
    [TestFixture]
    public class EditorStateCommandHandlerTests
    {
        [Test]
        public void Route_EditorStop_ReturnsEditMode()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "editor-stop",
                    Command = "editor.stop",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as EditorStateCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual("edit", result.PlayMode);
        }

        [Test]
        public void Route_EditorPause_InEditMode_ReturnsError()
        {
            CommandRouter.Route(
                new CommandRequest
                {
                    Id = "editor-stop-before-pause",
                    Command = "editor.stop",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "editor-pause",
                    Command = "editor.pause",
                    ParamsJson = "{}",
                },
                executeEnabled: true);

            Assert.IsFalse(response.Success);
            StringAssert.Contains("cannot pause", response.Error.ToLowerInvariant());
        }
    }
}
