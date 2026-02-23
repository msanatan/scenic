using NUnit.Framework;
using Scenic.Editor;
using Scenic.Editor.Commands.Scene;

namespace Scenic.Editor.Tests.Commands.Scene
{
    [TestFixture]
    public class SceneListCommandHandlerTests
    {
        [Test]
        public void Route_SceneList_ReturnsPaginatedScenePaths()
        {
            var response = CommandRouter.Route(
                new CommandRequest
                {
                    Id = "scene-list",
                    Command = "scene.list",
                    ParamsJson = "{\"limit\":10,\"offset\":0}",
                },
                executeEnabled: true);

            Assert.IsTrue(response.Success);
            var result = response.Result as SceneListCommandResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(10, result.Limit);
            Assert.AreEqual(0, result.Offset);
            Assert.GreaterOrEqual(result.Total, 1);
            Assert.IsNotNull(result.Scenes);
            if (result.Scenes.Length > 0)
            {
                Assert.IsNotNull(result.Scenes[0].Name);
                Assert.IsNotNull(result.Scenes[0].Path);
                StringAssert.EndsWith(".unity", result.Scenes[0].Path);
            }
        }
    }
}
